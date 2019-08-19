require('dotenv').config()

const validator = require('./validation/messageValidator')
const s3 = require('./s3/operations')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const esMessageSender = require('./search/esMessageSender')

exports.handler = async function (message, context, callback) {
  console.log('Running Datahub Ingester')

  if (message.config.action === 'publish') {
    const { valid, errors } = validator.validatePublishOrRedindexMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Publishing record with id ${message.asset.id}`)
    await publishToHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 's3-publish') {
    const { valid, errors } = validator.validateS3PublishMessage(message)

    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }

    var s3BucketName = message.config.s3.bucketName
    var s3ObjectKey = decodeURIComponent(message.config.s3.objectKey.replace(/\+/g, " "));

    var response = await s3.getMessage(s3BucketName, s3ObjectKey).catch((error) => {
      callback(new Error(`Failed to retrieve S3 message`, error))
    })
    var s3Message = JSON.parse(response.Body.toString())
    console.log(`Retrieved message for record id ${s3Message.asset.id}`)

    const { s3MessageValid, s3MessageErrors } = validator.validatePublishOrRedindexMessage(s3Message)
    if (!s3MessageValid) {
      callback(JSON.stringify(s3MessageErrors, null, 2))
    }

    console.log(`Publishing record with id ${s3Message.asset.id}`)
    await publishToHub(s3Message, callback)
    callback(null, s3Message)
  }

  if (message.config.action === 'unpublish') {
    const { valid, errors } = validator.validateDeleteMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Unpublishing record with id ${message.asset.id}`)
    await unpublishFromHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 'reindex') {
    const { valid, errors } = validator.validatePublishOrRedindexMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Indexing record with id ${message.asset.id}`)
    await reindexFromMessage(message, callback)

    // truncate any base64 fields before returning them in the output
    message.asset.data = message.asset.data.map((resource) => {
      if (resource.http.fileBase64 !== undefined) {
        resource.http.fileBase64 = `${resource.http.fileBase64.substring(0, 10)}...`
      }

      return resource
    })
    callback(null, message)
  }

  callback(new Error(`Unknown Action type, expected [publish | unpublish | reindex] but got [${message.config.action}]`))
}

async function publishToHub (message, callback) {
  // Put new record onto Dynamo handler without base64 encodings
  console.log(`Saving asset in dynamo db`)
  var dynamoMessage = JSON.parse(JSON.stringify(message))
  dynamoMessage.asset.data.forEach(resource => {
    resource.http.fileBase64 = null
  })
  await dynamo.putAsset(dynamoMessage).catch((error) => {
    callback(new Error(`Failed to put asset into DynamoDB Table: ${error}`))
  })

  // Check the asset and its linked data structures exist, generate messages
  // required to be sent into
  console.log(`Creating queue messages`)
  var { success: createSuccess, sqsMessages, errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!createSuccess) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(', ')}]`))
  } else {
    console.log(`Created ${sqsMessages.length} SQS messages`)
  }

  // Delete any existing data in search index
  console.log(`Deleting from elasticsearch`)
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)
  // Send new indexing messages
  console.log(`Sending to elasticsearch`)
  var { success: sendSuccess, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!sendSuccess) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(', ')}"`))
  }
}

async function unpublishFromHub (message, callback) {
  // Delete any existing data in search index
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)
  // Remove asset from dynamo
  await dynamo.deleteAsset(message.asset.id, message.config.dynamo.table).catch((err) => {
    callback(err)
  })
}

async function reindexFromMessage (message, callback) {
  // Check the asset and its linked data structures exist, generate messages
  // required to be sent into
  var { success: createSuccess, sqsMessages, errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!createSuccess) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(', ')}]`))
  }
  // Remove existing index
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)

  var { success: sendSuccess, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!sendSuccess) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(', ')}"`))
  }
}

async function deleteFromElasticsearch (id, index, callback) {
  console.log(`Elasticsearch - Removing record with id '${id}' and all records with that as a parent_id in index '${index}'`)
  var { success, messages } = await esMessageSender.deleteById(id, index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${id}, DynamoDB record still exists: ${messages.join(', ')}`))
  }
}
