
const AWS = require('aws-sdk')

const dynamo = new AWS.DynamoDB.DocumentClient()

var getTable = function(env) {
  var table = /live/.test(env) && 'datahub-live-assets' ||
              /beta/.test(env) && 'datahub-beta-assets' ||
              'unknown';

  if (table === 'unknown') {
    throw "Invalid environment name"
  }

  return table
}

module.exports.putAsset = function(req) {

  // you can get to the lambda context like this:
  // req.lambdaContext.awsRequestId

  // log something to cloudwatch
  console.log('Hello from putAsset')
  console.log(`PUTting asset ${req.body.id} into ${req.pathParams.env} environment`)

  table = getTable(req.pathParams.env)

  var params = {  
    TableName: table,  
    Item: req.body
  }

  // put the asset into the database
  return dynamo.put(params).promise()
}

module.exports.deleteAsset = function(req) {

  // log something to cloudwatch
  console.log('Hello from deleteAsset')
  console.log(`DELETEing asset ${req.body.id} from ${req.pathParams.env} environment`)

  table = getTable(req.pathParams.env)

  var params = {  
    TableName: table,  
    Key: { id: req.body.id }
  }

  // put the asset into the database
  return dynamo.delete(params).promise()
}

module.exports.scanAssets = async function (req) {
  table = getTable(req.pathParams.env)
  let response = await dynamo.scan({ TableName: table }).promise()
  return response.Items
}
