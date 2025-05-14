import { USE_LOCALSTACK } from '../env'
import uuid4 from 'uuid/v4'
import { S3 } from '@aws-sdk/client-s3'

const s3 = new S3({
  endpoint: USE_LOCALSTACK ? new AWS.Endpoint('http://localhost:4572') : undefined,
  s3ForcePathStyle: USE_LOCALSTACK
})

export async function uploadMessageToS3 (message, config) {
  var errors = []
  var s3Key = uuid4()
  var bucket = config.sqs.largeMessageBucket
  var params = {
    Bucket: bucket,
    Key: s3Key,
    Body: JSON.stringify(message)
  }

  console.log(`S3 - Saving large message for doc ${message.document.id} with title ${message.document.title} and asset ID ${message.document.asset_id} at ${s3Key} in bucket ${bucket}`)

  await s3.putObject(params).promise().catch((error) => {
    console.error(`Failed to put message ${message.document.id} into S3 with key ${s3Key}, error: ${error}`)
    errors.push(error)
  })

  return { success: errors.length === 0, errors: errors, s3Key: s3Key }
}