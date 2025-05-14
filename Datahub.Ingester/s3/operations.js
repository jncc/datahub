import { S3Client } from "@aws-sdk/client-s3";
import { default as env } from '../env.js';

var s3Client = null

function getClient () {
  if (s3Client === null) {
    if (env.USE_LOCALSTACK) {
      s3Client = new S3Client({ endpoint: 'http://localhost:4572' })
    } else {
      s3Client = new S3Client()
    }
  }
  return s3Client
}

export function getMessage (bucketName, objectKey) {
  console.log(`S3 - Get ${objectKey} from ${bucketName} bucket`)
  var params = {
    Bucket: bucketName,
    Key: objectKey
  }

  return getClient().getObject(params).promise()
}
