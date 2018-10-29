
const AWS = require('aws-sdk')

const dynamo = new AWS.DynamoDB.DocumentClient()

module.exports.putAsset = function(req) {

  // log something to cloudwatch
  console.log(req.lambdaContext.awsRequestId)

  var params = {  
    TableName: 'datahub-assets',  
    Item: {
        id: req.body.asset.id,
        // other properties go here
    } 
  }

  // put the asset into the database
  return dynamo.put(params).promise()
}

module.exports.scanAssets = async function (req) {
  let response = await dynamo.scan({ TableName: 'datahub-assets' }).promise()
  return response.Items
}
