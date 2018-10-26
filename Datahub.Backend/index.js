
// const AWS = require('aws-sdk');

// let dynamo = new AWS.DynamoDB.DocumentClient();

// exports.upsert = async function (event, context) {

// 	// log to cloudwatch
// 	console.log(event);

// 	upsertIntoDynamo(event);

// 	context.succeed('Received ' + event.id);
// };

// let upsertIntoDynamo = function(event) {
// 	// todo
// }

const AWS = require('aws-sdk')
const ClaudiaApiBuilder = require('claudia-api-builder')

var api = new ClaudiaApiBuilder()
var dynamo = new AWS.DynamoDB.DocumentClient()

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
		};

		// put into the database
    return dynamo.put(params).promise();
  },
  { success: 201 }
)

api.get('/assets',
  async function (req) {
    // return dynamo.scan({ TableName: 'datahub-assets' })
    //   .promise()
    //   .then(response => response.Items)
		let response = await dynamo.scan({ TableName: 'datahub-assets' }).promise()
		return response.Items
  }
);

module.exports = api
