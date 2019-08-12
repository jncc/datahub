require('dotenv').config()

const validator = require('./validation/messageValidator')
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
  // Check the asset and its linked data structures exist, generate messages
  // required to be sent into
  var { success: createSuccess, sqsMessages, errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!createSuccess) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(', ')}]`))
  }

  // Put new record onto Dynamo handler without base64 encodings
  var dynamoMessage = JSON.parse(JSON.stringify(message.asset))
  dynamoMessage.data.forEach(resource => {
    resource.http.fileBase64 = null
  })
  await dynamo.putAsset(dynamoMessage).catch((error) => {
    callback(new Error(`Failed to put asset into DynamoDB Table: ${error}`))
  })

  // Delete any existing data in search index
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)
  // Send new indexing messages
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
