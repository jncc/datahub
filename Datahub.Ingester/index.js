require('dotenv').config()

const sizeof = require('object-sizeof')

const validator = require('./validation/messageValidator')
const s3 = require('./s3/operations')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const lambdaInvoker = require('./search/lambdaInvoker')
const s3MessageUploader = require('./search/s3MessageUploader')

const maxMessageSize = 256000 //256KB

exports.handler = async function (message, context, callback) {
  console.log('Running Datahub Ingester')
  console.log(`Received message: ${JSON.stringify(message)}`)

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
    const { valid, errors } = validator.validateUnpublishMessage(message)
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
  // Check the asset and its linked data structures exist, generate SQS messages
  var { success: createSuccess, sqsMessages, errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!createSuccess) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(', ')}]`))
  }

  // Check if messages need base64 content adding and save large messages to S3
  var messageBodies = []
  for (var sqsMessage of sqsMessages) {
    console.log('Looping through sqsMessages')
    var messageBody = sqsMessage

    if (sqsMessageBuilder.fileTypeIsIndexable(sqsMessage.document.file_extension)) {
      console.log('File is a PDF')
      if (sqsMessage.document.file_base64 === undefined) {
        callback(new Error(`Base64 content not provided for PDF resource for ${sqsMessage.document.title}`))
      }

      console.log('Large message, saving to S3')
      var bucket = message.config.sqs.largeMessageBucket
      var { success: uploadSuccess, uploadErrors, s3Key } = await s3MessageUploader.uploadMessageToS3(sqsMessage, message.config)
      if (!uploadSuccess) {
        callback(new Error(`Failed to upload S3 message with the following errors: [${uploadErrors.join(', ')}]`))
      } else {
        console.log(`Successfully saved S3 message to ${s3Key} in bucket ${bucket}`)
      }

      // message body now points to S3 location
      messageBody = {
        s3BucketName: bucket,
        s3Key: s3Key
      }
    }

    messageBodies.push(messageBody)
  }

  // Put new record onto Dynamo handler without base64 encodings
  if (message.asset.data) {
    message.asset.data.forEach(resource => {
      resource.http.fileBase64 = null
    })
  }
  await dynamo.putAsset(message).catch((error) => {
    callback(new Error(`Failed to put asset into DynamoDB Table: ${error}`))
  })

  // Delete any existing data in search index
  await deleteFromOpensearch(message.asset.id, message.config.elasticsearch.index, callback)

  // Send new indexing messages
  var { success: sendSuccess, messages } = await sqsMessageSender.sendMessages(messageBodies, message.config)
  if (!sendSuccess) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(', ')}"`))
  }
}

async function unpublishFromHub (message, callback) {
  // Delete any existing data in search index
  await deleteFromOpensearch(message.asset.id, message.config.elasticsearch.index, callback)
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
  await deleteFromOpensearch(message.asset.id, message.config.elasticsearch.index, callback)

  var { success: sendSuccess, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!sendSuccess) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(', ')}"`))
  }
}

async function deleteFromOpensearch (id, index, callback) {
  console.log(`Opensearch - Removing records with asset_id '${id}' in index '${index}'`)
  var { success, messages } = await lambdaInvoker.deleteByAssetId(id, index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${id}: ${messages.join(', ')}`))
  }
}
