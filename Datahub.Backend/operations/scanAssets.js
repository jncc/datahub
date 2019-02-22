
const AWS = require('aws-sdk')

const dynamo = new AWS.DynamoDB.DocumentClient()

module.exports.scanAssets = async function (req) {
  let response = await dynamo.scan({ TableName: 'datahub-assets' }).promise()
  return response.Items
}
