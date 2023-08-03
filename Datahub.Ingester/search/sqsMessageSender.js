

const env = require('../env')

const AWS = require('aws-sdk')

module.exports.sendMessages = async function (messages, config) {
  var errors = []
  var sqs = new AWS.SQS()

  console.log(`SQS - ${messages.length} messages to send`)

  for (var message of messages) {
    var params = {
      MessageBody: JSON.stringify(message),
      QueueUrl: config.sqs.queueEndpoint
    }

    console.log(`SQS - Sending message ${JSON.stringify(message)} to endpoint ${config.sqs.queueEndpoint}`)
    await sqs.sendMessage(params).promise().catch((error) => {
      console.error(`Message was not sent to the queue successfully: ${error}`)
      errors.push(`Message was not sent to the queue successfully: ${error}`)
    })
  }

  return { success: errors.length === 0, messages: errors }
}
