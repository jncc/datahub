import { DynamoDBClient, ScanCommand } from "@aws-sdk/client-dynamodb";
import {
  DynamoDBDocumentClient,
  ScanCommand
} from "@aws-sdk/lib-dynamodb";

const client = new DynamoDBClient({});
const dynamo = DynamoDBDocumentClient.from(client);
const tableName = process.env.TABLE_NAME;

export const handler = async (event, context) => {
  let body = {};
  let statusCode = 200;
  const headers = {
    "Content-Type": "application/json",
  };

  try {
    switch (event.routeKey) {
      case "GET /get-all":
        scanResults = await dynamo.send(
          new ScanCommand({
            TableName: tableName,
            AttributesToGet: [
              "id", "data", "timestamp_utc"
            ]
          })
        )
        
        body = {
          count: scanResults.Count,
          items: scanResults.items.map(function callback(element, index, array){
            return {
              id: element.id,
              title: element.title,
              timestamp: element.timestamp_utc
            }
          })
        }
        
        break;
      default:
        throw new Error("Not Found");
    }
  } catch (err) {
    statusCode = 404;
    body.message = err.message;
  } finally {
    body = JSON.stringify(body);
  }

  return {
    statusCode,
    body,
    headers,
  };
};
