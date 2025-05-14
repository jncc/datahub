import { DynamoDBClient } from "@aws-sdk/client-dynamodb"
import { DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb"
import { default as env } from '../env.js'

let dynamo = null

function getClient () {
  if (dynamo === null) {
    if (env.USE_LOCALSTACK) {
      dynamo = DynamoDBDocumentClient.from(new DynamoDBClient({ endpoint: 'http://localhost:4569' }))
    } else {
      dynamo = DynamoDBDocumentClient.from(new DynamoDBClient({ }))
    }
  }
  return dynamo
}

export function putAsset (message) {
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

export function deleteAsset (id, table) {
  console.log(`DynamoDB - DELETE asset ${id} from ${table} table`)

  var params = {
    TableName: table,
    Key: { id: id }
  }

  // delete the asset from the database
  return getClient().delete(params).promise()
}

export async function getAsset (id, table) {
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
