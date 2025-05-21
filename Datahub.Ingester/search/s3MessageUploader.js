import { default as env } from '../env.js'
import { v4 as uuid4 } from 'uuid'
import { PutObjectCommand, S3Client, S3ServiceException } from '@aws-sdk/client-s3'

const s3 = new S3Client({
  endpoint: env.USE_LOCALSTACK ? new AWS.Endpoint('http://localhost:4572') : undefined,
  s3ForcePathStyle: env.USE_LOCALSTACK
})

export async function uploadMessageToS3 (message, config) {
  var errors = []
  var s3Key = uuid4()
  var bucket = config.sqs.largeMessageBucket

  console.log(`S3 - Saving large message for doc ${message.document.id} with title ${message.document.title} and asset ID ${message.document.asset_id} at ${s3Key} in bucket ${bucket}`)

  const command = new PutObjectCommand({
    Bucket: bucket,
    Key: s3Key,
    Body: JSON.stringify(message)
  })

  try {  
    await s3.send(command)
  } catch (error) {
    if (error instanceof S3ServiceException) {
      console.error(`S3 - Failed to put message ${message.document.id} into S3 with key ${s3Key}, error: ${error}`)
      errors.push(error)
    } else {
      console.error(`S3 - Unexpected error: ${error}`)
      errors.push(error)
    }
  }

  return { success: errors.length === 0, errors: errors, s3Key: s3Key }
}