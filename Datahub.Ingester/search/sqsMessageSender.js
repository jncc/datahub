const maxMessageSize = 256000

const env = require('../env')
const uuid4 = require('uuid/v4')
const sizeof = require('object-sizeof')

const AWS = require('aws-sdk')
const s3 = new AWS.S3({
  endpoint: env.USE_LOCALSTACK ? new AWS.Endpoint('http://localhost:4572') : undefined,
  s3ForcePathStyle: env.USE_LOCALSTACK ? true : false
})

module.exports.sendMessages = async function (messages, config) {
  var errors = []
  var sqs = new AWS.SQS()

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
        s3Bucket: config.sqs.largeMessageBucket,
        s3Key: msgId
      })
    }

    if (messageCreated) {
      console.log(`SQS - Sending message for ${message.document.id}`)
      await sqs.sendMessage(params, function (error, data) {
        if (error) {
          console.error(`Failed to send message to SQS: ${error}`)
          errors.push(error)
        }
      }).promise()
      //.then((resp) => {console.log(resp, params)})
    }
  }

  return { success: errors.length === 0, messages: errors }
}

function uploadFileToS3 (message, id, bucket) {
  const params = {
    Bucket: bucket,
    Key: id,
    Body: JSON.stringify(message)
  }
  return s3.putObject(params).promise()
}
