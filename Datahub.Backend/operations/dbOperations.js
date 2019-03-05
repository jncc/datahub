
const AWS = require('aws-sdk')

const dynamo = new AWS.DynamoDB.DocumentClient()

module.exports.putAsset = function(req) {

  // you can get to the lambda context like this:
  // req.lambdaContext.awsRequestId

  // log something to cloudwatch
  console.log('Hello from putAsset')
  console.log(`PUTting asset ${req.body.id}`)

  var params = {  
    TableName: 'datahub-live-assets',  
    Item: req.body
  }

  // put the asset into the database
  return dynamo.put(params).promise()
}

module.exports.deleteAsset = function(req) {

  // log something to cloudwatch
  console.log('Hello from deleteAsset')
  console.log(`DELETEing asset ${req.body.id}`)

  var params = {  
    TableName: 'datahub-live-assets',  
    Key: { id: req.body.id }
  }

  // put the asset into the database
  return dynamo.delete(params).promise()
}

module.exports.scanAssets = async function (req) {
  let response = await dynamo.scan({ TableName: 'datahub-live-assets' }).promise()
  return response.Items
}
