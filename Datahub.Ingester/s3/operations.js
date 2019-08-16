const AWS = require('aws-sdk')
const env = require('../env')

var s3Client = null

function getClient () {
  if (s3Client === null) {
    if (env.USE_LOCALSTACK) {
      s3Client = new AWS.S3({ endpoint: 'http://localhost:4572' })
    } else {
      s3Client = new AWS.S3()
    }
  }
  return s3Client
}

module.exports.getMessage = async function (bucketName, objectKey) {
  console.log(`S3 - Get ${objectKey} from ${bucketName} bucket`)
  var params = {
    Bucket: bucketName,
    Key: objectKey
  }

  return getClient().getObject(params, (err, data) => {
    if (err) {
      console.error(err)
    }
  }).promise()
}
