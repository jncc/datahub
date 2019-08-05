require('dotenv').config()

const validator = require('./validation/messageValidator')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const esMessageSender = require('./search/esMessageSender')

exports.handler = async function (message, context, callback) {
  console.log('Running Datahub Ingester')
  var { valid: valid, errors: errors } = validator.validatePublishMessage(message)
  if (!valid) {
    callback(JSON.stringify(errors, null, 2))
  }

  if (message.config.action === 'publish') {
    console.log(`Publishing record with id ${message.asset.id}`)
    await publishToHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 'unpublish') {
    console.log(`Publishing record with id ${message.asset.id}`)
    await unpublishFromHub(message, callback)
    callback(null, message)
  }

  callback(new Error(`Uknown Action type, expected [publish | unpublish] but got [${message.config.action}]`))
}

async function publishToHub (message, callback) {
  // Check the asset and its linked data structures exist, generate messages
  // required to be sent into
  var { success: success, sqsMessages: sqsMessages, errors: errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!success) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(", ")}]`))
  }

  // Put new record onto Dynamo handler
  await dynamo.putAsset(message).catch((error) => {
    callback(new Error(`Failed to put asset into DynamoDB Table: ${error}`))
  })

  // Delete any existing data in search index
  var { success: success, messages: messages } = await esMessageSender.deleteById(message.asset.id, message.config.elasticsearch.index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${message.asset.id}, but new DynamoDB record was inserted: ${messages.join(", ")}`))
  }
  // Send new indexing messages
  var { success, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!success) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(", ")}"`))
  }

  callback(null, message)
}

async function unpublishFromHub (message, callback) {
  // Delete any existing data in search index
  var { success: success, messages: messages } = await esMessageSender.deleteById(message.asset.id, message.config.elasticsearch.index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${message.asset.id}, DynamoDB record still exists: ${messages.join(", ")}`))
  }
  // Remove asset from dynamo
  await dynamo.deleteAsset(message).catch((err) => {
    callback(err)
  })
}
