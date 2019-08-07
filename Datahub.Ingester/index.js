require('dotenv').config()

const validator = require('./validation/messageValidator')
const dynamo = require('./dynamo/operations')
const sqsMessageBuilder = require('./search/sqsMessageBuilder')
const sqsMessageSender = require('./search/sqsMessageSender')
const esMessageSender = require('./search/esMessageSender')

exports.handler = async function (message, context, callback) {
  console.log('Running Datahub Ingester')

  if (message.config.action === 'publish') {
    var { valid: valid, errors: errors } = validator.validatePublishMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Publishing record with id ${message.asset.id}`)
    await publishToHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 'unpublish') {
    var { valid: valid, errors: errors } = validator.validateDeleteOrReIndexMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Unpublishing record with id ${message.asset.id}`)
    await unpublishFromHub(message, callback)
    callback(null, message)
  }

  if (message.config.action === 'reindex') {
    var { valid: valid, errors: errors } = validator.validateDeleteOrReIndexMessage(message)
    if (!valid) {
      callback(JSON.stringify(errors, null, 2))
    }
    console.log(`Indexing record with id ${message.asset.id}`)
    await reindexFromHub(message, callback)
    callback(null, message)
  }

  callback(new Error(`Uknown Action type, expected [publish | unpublish | reindex] but got [${message.config.action}]`))
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
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)
  // Send new indexing messages
  var { success, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!success) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(", ")}"`))
  }

  callback(null, message)
}

async function unpublishFromHub (message, callback) {
  // Delete any existing data in search index
  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)
  // Remove asset from dynamo
  await dynamo.deleteAsset(message.asset.id, message.config.dynamo.table).catch((err) => {
    callback(err)
  })
}

async function reindexFromHub (message, callback) {
  var document = await dynamo.getAsset(message.asset.id, message.config.dynamo.table)
    .catch((error) => {
      callback(error)
    })

  // check if return is ok
  if (isEmptyObject(document) || document === undefined) {
    callback("DynamoDB - No record found with the provided ID")
  }

  console.log('DynamoDB - Retrieved document successfully')

  await deleteFromElasticsearch(message.asset.id, message.config.elasticsearch.index, callback)

  message.asset = {
    id: document.Item.id,
    metadata: {
      title: document.Item.metadata.title,
      topicCategory: document.Item.metadata.topicCategory,
      resourceType: document.Item.metadata.resourceType,
      keywords: document.Item.metadata.keywords,
      abstract: document.Item.metadata.abstract,
      responsibleOrganisation: document.Item.metadata.responsibleOrganisation,
      metadataPointOfContact: document.Item.metadata.metadataPointOfContact
    },
    data: document.Item.data
  }

  var { success: success, sqsMessages: sqsMessages, errors: errors } = await sqsMessageBuilder.createSQSMessages(message)
  if (!success) {
    callback(new Error(`Failed to create SQS messages with the following errors: [${errors.join(", ")}]`))
  }

  var { success, messages } = await sqsMessageSender.sendMessages(sqsMessages, message.config)
  if (!success) {
    callback(new Error(`Failed to send records to search index SQS queue, but new dynamoDB record was inserted and old search index records were deleted: "${messages.join(", ")}"`))
  }
}

function isEmptyObject (obj) {
  return !Object.keys(obj).length
}

async function deleteFromElasticsearch (id, index, callback) {
  var { success: success, messages: messages } = await esMessageSender.deleteById(id, index)
  if (!success) {
    callback(new Error(`Failed to delete old search index records for asset ${id}, DynamoDB record still exists: ${messages.join(", ")}`))
  }
}
