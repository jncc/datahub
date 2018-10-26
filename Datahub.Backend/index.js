
const AWS = require('aws-sdk')
const ClaudiaApiBuilder = require('claudia-api-builder')

const api = new ClaudiaApiBuilder()
const dynamo = new AWS.DynamoDB.DocumentClient()

api.get('/hello', () => 'Hello!')

api.post('/assets',
  function (req) {

		// log something to cloudwatch
		console.log(req.lambdaContext.awsRequestId)

    var params = {  
      TableName: 'datahub-assets',  
      Item: {
          id: req.body.asset.id,
          // other properties go here
      } 
		}

		// put into the database
    return dynamo.put(params).promise()
  },
  {
    success: 201,
    authorizationType: 'AWS_IAM',
  }
)

api.get('/assets',
  async function (req) {
		let response = await dynamo.scan({ TableName: 'datahub-assets' }).promise()
		return response.Items
  }
)

module.exports = api
