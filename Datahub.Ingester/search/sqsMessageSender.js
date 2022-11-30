const S3 = require('@aws-sdk/client-s3')
const SQS = require('@aws-sdk/client-sqs')
const env = require('../env')
const uuid4 = require('uuid/v4')
const sizeof = require('object-sizeof')
const maxMessageSize = 256000

const s3 = new S3.S3({
  endpoint: env.USE_LOCALSTACK ? 'http://localhost:4566' : undefined,
  s3ForcePathStyle: env.USE_LOCALSTACK
})

async function sendMessages(messages, config) {
  var errors = []
  var sqs = new SQS.SQS()

  for (var id in messages) {
    var message = messages[id]
    var params = {
      MessageBody: JSON.stringify(message),
      QueueUrl: config.sqs.queueEndpoint
    }

    var messageCreated = true

    var largeMessage = sizeof(message) > maxMessageSize
    if (largeMessage) {
      console.log(`S3 - Creating large message for ${message.document.id}`)
      var msgId = uuid4()
      await uploadFileToS3(message, msgId, config.sqs.largeMessageBucket).catch((error) => {
        console.error(`Failed to put message ${message.document.id} into S3: ${error}`)
        errors.push(error)
        messageCreated = false
      })
      params.MessageBody = JSON.stringify({
        s3BucketName: config.sqs.largeMessageBucket,
        s3Key: msgId
      })
    }

    if (messageCreated) {
      console.log(`SQS - Sending message for ${message.document.id}`)
      await sqs.sendMessage(params).promise().catch((error) => {
        console.error(`Message was not sent to the queue successfully: ${error}`)
        errors.push(`Message was not sent to the queue successfully: ${error}`)
      })
    }
  }

  return { success: errors.length === 0, messages: errors }
}

function uploadFileToS3(message, id, bucket) {
  const params = {
    Bucket: bucket,
    Key: id,
    Body: JSON.stringify(message)
  }
  return s3.putObject(params).promise()
}
