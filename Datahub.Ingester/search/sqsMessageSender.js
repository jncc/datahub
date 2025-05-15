import { SQSClient, SendMessageCommand } from "@aws-sdk/client-sqs"

export async function sendMessages (messages, config) {
  var errors = []
  const sqs = new SQSClient({})

  console.log(`SQS - ${messages.length} messages to send`)

  for (var message of messages) {
    const command = new SendMessageCommand({
      MessageBody: JSON.stringify(message),
      QueueUrl: config.sqs.queueEndpoint
    })

    console.log(`SQS - Sending message ${JSON.stringify(message)} to endpoint ${config.sqs.queueEndpoint}`)
    await sqs.send(command).catch((error) => {
      console.error(`Message was not sent to the queue successfully: ${error}`)
      errors.push(`Message was not sent to the queue successfully: ${error}`)
    })
  }

  return { success: errors.length === 0, messages: errors }
}
