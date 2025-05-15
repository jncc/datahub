import { default as env } from '../env.js'
import { LambdaClient, InvokeCommand } from "@aws-sdk/client-lambda";

export async function deleteByAssetId (id, index) {
  var errors = []

  const lambda = new LambdaClient()
  const command = new InvokeCommand({
    FunctionName: env.SEARCH_DELETER_LAMBDA,
    InvocationType: 'RequestResponse',
    LogType: 'Tail',
    Payload: `{ "id": "${id}", "index": "${index}" }`
  })

  console.log(`Invoking lambda ${env.SEARCH_DELETER_LAMBDA}`)
  await lambda.send(command).catch((error) => {
    console.error(`Lambda ${env.SEARCH_DELETER_LAMBDA} delete by asset ID '${id}' request failed: ${error}`)
    errors.push(error)
  })

  return { success: errors.length === 0, messages: errors }
}
