import { DynamoDBClient } from "@aws-sdk/client-dynamodb"
import { DynamoDBDocumentClient, GetCommand, DeleteCommand, PutCommand } from "@aws-sdk/lib-dynamodb"
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

  const item = {
    ...message.asset,
    timestamp_utc: new Date().toISOString()
  }

  const command = new PutCommand({
    TableName: message.config.dynamo.table,
    Item: item
  })

  // put the asset into the database
  return getClient().send(command)
}

export function deleteAsset (id, table) {
  console.log(`DynamoDB - DELETE asset ${id} from ${table} table`)

  const command = new DeleteCommand({
    TableName: table,
    Key: { id: id }
  })

  // delete the asset from the database
  return getClient().send(command)
}

export async function getAsset (id, table) {
  console.log(`DynamoDB - GET asset ${id} from ${table} table`)
  const command = {
    TableName: table,
    Key: { id: id }
  }

  return getClient().send(command)
}
