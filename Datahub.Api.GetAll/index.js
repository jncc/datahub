import { DynamoDBClient, ScanCommand } from "@aws-sdk/client-dynamodb";
import {
  DynamoDBDocumentClient
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
        let scanResults = [];
        let items;
        let params = {
          TableName: tableName,
          AttributesToGet: [
            "id", "timestamp_utc"
          ]
        };
        
        do {
          items = await dynamo.send(new ScanCommand(params));
          items.Items.forEach((item) => scanResults.push(item));
          params.ExclusiveStartKey = items.LastEvaluatedKey;
        } while (typeof items.LastEvaluatedKey != "undefined");
        
        body = {
          count: scanResults.length,
          items: scanResults.map(function callback(element, index, array){
            return {
              id: element.id.S,
              timestamp: element.timestamp_utc.S
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
