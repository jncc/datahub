require('dotenv').config()

const validator = require('./validation/messageValidator')
const s3 = require('./s3/operations')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const esMessageSender = require('./search/esMessageSender')
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
  // Check the asset and its linked data structures exist, generate SQS messages
  var { success: createSuccess, sqsMessages, errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!createSuccess) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(', ')}]`))
  }

  // Check if messages need base64 content adding and save large messages to S3
  var messageBodies = []
  for (var sqsMessage of sqsMessages) {
    var messageBody = sqsMessage

    if (sqsMessageBuilder.fileTypeIsIndexable(sqsMessage.file_extension)) {
      console.log(`Adding base64 file content to SQS message`)
      var clonedMessage = JSON.parse(JSON.stringify(sqsMessage))
      var { success: addBase64Success, addBase64Errors, messageWithBase64Content } = await addBase64FileContent(message, clonedMessage)
      if (!addBase64Success) {
        callback(new Error(`Failed to add base64 content with the following errors: [${addBase64Errors.join(', ')}]`))
      }

      // check if the message is now too large, if it is then save to S3
      var largeMessage = sizeof(messageWithBase64Content) > maxMessageSize
      if (largeMessage) {
        var { success: uploadSuccess, uploadErrors, s3Key } = await s3MessageUploader.uploadMessageToS3(messageWithBase64Content, message.config)
        if (!uploadSuccess) {
          callback(new Error(`Failed to upload S3 message with the following errors: [${uploadErrors.join(', ')}]`))
        }

        // message body now points to S3 location
        messageBody = {
          s3BucketName: message.config.sqs.largeMessageBucket,
          s3Key: s3Key
        }
      } else {
        messageBody = messageWithBase64Content
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
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)

  // Send new indexing messages
  var { success: sendSuccess, messages } = await sqsMessageSender.sendMessages(messageBodies, message.config)
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
  console.log(`Elasticsearch - Removing records with asset_id '${id}' in index '${index}'`)
  var { success, messages } = await esMessageSender.deleteByAssetId(id, index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${id}, DynamoDB record still exists: ${messages.join(', ')}`))
  }
}

async function addBase64FileContent (message, sqsMessage) {
  var resource = message.asset.data.find(o => o.title === sqsMessage.document.title)
  var errors = []

  if (resource.http.fileBase64 === undefined) { // if not already provided then download the file
    await sqsMessageBuilder.getBase64ForFile(resource.http.url).then((response) => {
      sqsMessage.document.file_base64 = Buffer.from(response.data, 'binary').toString('base64')
    }).catch((error) => {
      errors.push(error)
      return { success: false, errors: errors, messageWithBase64Content: null }
    })
  } else {
    sqsMessage.document.file_base64 = resource.http.fileBase64
  }

  return { success: true, errors: errors, messageWithBase64Content: sqsMessage }
}
