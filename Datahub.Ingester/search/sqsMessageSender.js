const maxMessageSize = 256000

const env = require('../env')
const uuid4 = require('uuid/v4')
const sizeof = require('object-sizeof')

const AWS = require('aws-sdk')
const s3 = new AWS.S3({
  endpoint: env.USE_LOCALSTACK ? 'http://localhost:4572' : undefined
})

module.exports.sendMessages = async function (messages, config) {
  var errors = []
  var sqs = new AWS.SQS({ apiVersion: '2012-11-05' })

  for (var id in messages) {
    var message = messages[id]
    var params = {
      MessageBody: JSON.stringify(message),
      QueueUrl: env.SQS_ENDPOINT
    }

    var messageCreated = true
    console.log(sizeof(message))

    var largeMessage = sizeof(message) > maxMessageSize
    if (largeMessage) {
      console.log(`SQS - Creating large message for ${message.document.id}`)
      var msgId = uuid4()
      await uploadFileToS3(message, msgId).catch((error) => {
        console.error(`Failed to put message ${message.document.id} into S3: ${error}`)
        errors.push(error)
        messageCreated = false
      })
      params.MessageBody = JSON.stringify({
        s3Bucket: env.S3_BUCKET,
        s3Key: msgId
      })
    }

    if (messageCreated) {
      console.log(`SQS - Sending message for ${message.document.id}`)
      sqs.sendMessage(params, function (error, data) {
        if (error) {
          console.error(`Failed to send message to SQS: ${error}`)
          errors.push(error)
        } else {
          console.log(`Pushed message (${message.document.id}) to SQS Queue`)
        }
      })
    }
  }

  return { success: errors.length === 0, messages: errors }
}

function uploadFileToS3 (message, id) {
  const params = {
    Bucket: env.S3_BUCKET,
    Key: `${id}`,
    Body: JSON.stringify(message)
  }
  return s3.putObject(params).promise()
}
