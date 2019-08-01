const maxMessageSize = 256000

const env = require('../env')
const uuid4 = require('uuid/v4')

const AWS = require('aws-sdk')
const s3 = new AWS.S3()
AWS.config.update({ region: env.AWS_REGION })

module.exports.sendMessages = async function (messages, config) {
  var errors = []
  var sqs = new AWS.SQS({ apiVersion: '2012-11-05' })

  for (var message in messages) {
    var params = {
      MessageBody: message,
      QueueUrl: env.SQS_ENDPOINT
    }
    var messageCreated = true

    var largeMessage = Buffer.byteLength(Buffer.from(message)) > maxMessageSize
    if (largeMessage) {
      var msgId = uuid4()
      await uploadFileToS3(message, msgId).catch((error) => {
        console.error(`Failed to put message ${message.document.id} into S3`)
        console.error(error)
        errors.push(error)
        messageCreated = false
      })
      params.MessageBody = {
        s3Bucket: env.S3_BUCKET,
        s3Key: msgId
      }
    }

    if (messageCreated) {
      sqs.sendMessage(params, function (error, data) {
        if (error) {
          errors.push(error)
        } else {
          console.log(`Pushed message (${message.document.id}) to SQS Queue`)
        }
      })
    }
  }

  return (errors.length === 0, errors)
}

function uploadFileToS3 (message, id) {
  const params = {
    Bucket: env.S3_BUCKET,
    Key: id,
    Body: message
  }
  return s3.putObject(params).promise()
}
