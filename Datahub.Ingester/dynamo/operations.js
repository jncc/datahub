const dynamodbClient = require('@aws-sdk/client-dynamodb');
const credentialProvider = require('@aws-sdk/credential-provider-node');
const dynamodbLib = require('@aws-sdk/lib-dynamodb')
const env = require('../env')

var dynamo = null

const marshallOptions = {
  // Whether to automatically convert empty strings, blobs, and sets to `null`.
  convertEmptyValues: false, // false, by default.
  // Whether to remove undefined values while marshalling.
  removeUndefinedValues: false, // false, by default.
  // Whether to convert typeof object to map attribute.
  convertClassInstanceToMap: false, // false, by default.
};

const unmarshallOptions = {
  // Whether to return numbers as a string instead of converting them to native JavaScript numbers.
  wrapNumbers: false, // false, by default.
};

const translateConfig = { marshallOptions, unmarshallOptions };

function getClient () {
  if (dynamo === null) {
    var dynamoClientConfig = {
      region: env.AWS_REGION
    };

    if (env.AWS_PROFILE) {
      dynamoClientConfig['credentialDefaultProvider'] = credentialProvider.defaultProvider({
        profile: env.AWS_PROFILE
      })
    }

    if (env.USE_LOCALSTACK) {
      dynamoClientConfig['endpoint'] = 'http://localhost:4566';
    }

    ddbClient = new dynamodbClient.DynamoDBClient(dynamoClientConfig);
    dynamo = dynamodbLib.DynamoDBDocumentClient.from(ddbClient, translateConfig);    
  }
  return dynamo;
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
  return getClient().send(new dynamodbLib.PutCommand(params));
}

module.exports.deleteAsset = function (id, table) {
  console.log(`DynamoDB - DELETE asset ${id} from ${table} table`)

  var params = {
    TableName: table,
    Key: { id: id }
  }

  return getClient().send(new DeleteCommand(params));
}

module.exports.getAsset = async function (id, table) {
  console.log(`DynamoDB - GET asset ${id} from ${table} table`)
  var params = {
    TableName: table,
    Key: { id: id }
  }

  return getClient().send(new GetCommand(params));
}
