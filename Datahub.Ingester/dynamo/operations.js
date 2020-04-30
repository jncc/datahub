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
  console.log(`DynamoDB - PUT asset ${message.asset.id} into ${message.config.dynamo.table} table`)

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

module.exports.deleteAsset = function (id, table) {
  console.log(`DynamoDB - DELETE asset ${id} from ${table} table`)

  var params = {
    TableName: table,
    Key: { id: id }
  }

  // delete the asset from the database
  return getClient().delete(params).promise()
}

module.exports.getAsset = async function (id, table) {
  console.log(`DynamoDB - GET asset ${id} from ${table} table`)
  var params = {
    TableName: table,
    Key: { id: id }
  }

  return getClient().get(params, (error, data) => {
    if (error) {
      console.error(`DynamoDB - Unable to get the item: ${JSON.stringify(error, null, 2)}`)
    }
  }).promise()
}
