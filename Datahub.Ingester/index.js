require('dotenv').config()

const validator = require('./validation/messageValidator')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const esMessageSender = require('./search/esMessageSender')

exports.handler = async function (message, context, callback) {
  console.log('Running Datahub Ingester')
  var { valid, errors } = validator.validatePublishMessage(message)
  if (!valid) {
    callback(JSON.stringify(errors, null, 2))
  }

  if (message.config.action === 'publish') {
    console.log(`Publishing record with id ${message.asset.id}`)
    publishToHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 'unpublish') {
    console.log(`Publishing record with id ${message.asset.id}`)
    unpublishFromHub(message, callback)
    callback(null, message)
  }

  callback(new Error(`Uknown Action type, expected [publish | unpublish] but got [${message.config.action}]`))
}

async function publishToHub (message, callback) {
  // Check the asset and its linked data structures exist, generate messages
  // required to be sent into
  var sqsMessages = sqsMessageBuilder.createSQSMessages(message.asset)

  // Put new record onto Dynamo handler
  await dynamo.putAsset(message).catch((err) => {
    callback(err)
  })

  // Delete any existing data in search index
  esMessageSender.deleteById(message.asset.id, message.config.elasticsearch.index)
  // Send new indexing messages
  sqsMessageSender.sendMessages(sqsMessages, message.config)

  callback(null, message)
}

async function unpublishFromHub (message, callback) {
  // Delete any existing data in search index
  esMessageSender.deleteById(message.asset.id, message.config.elasticsearch.index)
  // Remove asset from dynamo
  await dynamo.deleteAsset(message).catch((err) => {
    callback(err)
  })
}
