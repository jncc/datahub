
const AWS = require('aws-sdk')
const env = require('../env')

// allow dev-time dynamo endpoint
const dynamoService = env.DYNAMO_ENDPOINT
  ? new AWS.DynamoDB({ endpoint: new AWS.Endpoint(DYNAMO_ENDPOINT) })
  : new AWS.DynamoDB()

const dynamo = new AWS.DynamoDB.DocumentClient({ service: dynamoService })

module.exports.putAsset = function(req) {

  // you can get to the lambda context like this:
  // req.lambdaContext.awsRequestId

  // log something to cloudwatch
  console.log('Hello from putAsset')

  var params = {  
    TableName: env.DYNAMO_TABLE,  
    Item: req.body.asset
  }

  // put the asset into the database
  return dynamo.put(params).promise()
}

module.exports.scanAssets = async function (req) {
  let response = await dynamo.scan({ TableName: 'datahub-assets' }).promise()
  return response.Items
}
