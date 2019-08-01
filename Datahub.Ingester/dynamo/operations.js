const AWS = require('aws-sdk')
const env = require('../env')

var dynamo = null

function getClient () {
  if (dynamo === null) {
    if (env.USE_LOCALSTACK) {
      dynamo = new AWS.DynamoDB.DocumentClient({ endpoint: 'http://localhost:4569' })
    } else {
      dynamo = new AWS.DynamoDB.DocumentClient()
    }
  }
  return dynamo
}

module.exports.putAsset = function (message) {
  // log something to cloudwatch
  console.log(`PUT asset ${message.asset.id} into ${message.config.dynamo.table} table`)

  var item = {
    ...message.asset,
    timestamp_utc: new Date().toISOString()
  }

  var params = {
    TableName: message.config.dynamo.table,
    Item: item
  }

  // put the asset into the database
  return getClient().put(params).promise()
}

module.exports.deleteAsset = function (id) {
  console.log(`DELETE asset ${message.asset.id} from ${message.config.dynamo.table} table`)

  var params = {
    TableName: message.config.dynamo.table,
    Key: { id: message.asset.id }
  }

  // delete the asset from the database
  return getClient().delete(params).promise()
}
