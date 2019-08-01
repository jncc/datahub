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

module.exports.putAsset = function (asset) {
  // log something to cloudwatch
  console.log(`PUT asset ${asset.id} into ${env.DYNAMODB_TABLE} table`)

  var item = {
    ...asset,
    timestamp_utc: new Date().toISOString()
  }

  var params = {
    TableName: env.DYNAMODB_TABLE,
    Item: item
  }

  // put the asset into the database
  return getClient().put(params).promise()
}

module.exports.deleteAsset = function (id) {
  console.log(`DELETE asset ${id} from ${env.DYNAMODB_TABLE} table`)

  var params = {
    TableName: env.DYNAMODB_TABLE,
    Key: { id: id }
  }

  // delete the asset from the database
  return getClient().delete(params).promise()
}
