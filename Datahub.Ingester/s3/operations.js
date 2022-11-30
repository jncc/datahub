const S3 = require("@aws-sdk/client-s3");
const env = require('../env')

var s3Client = null

function getClient () {
  if (s3Client === null) {
    if (env.USE_LOCALSTACK) {
      s3Client = new S3.S3({ endpoint: 'http://localhost:4566' })
    } else {
      s3Client = new S3.S3()
    }
  }
  return s3Client
}

module.exports.getMessage = function (bucketName, objectKey) {
  console.log(`S3 - Get ${objectKey} from ${bucketName} bucket`)
  var params = {
    Bucket: bucketName,
    Key: objectKey
  }

  return getClient().getObject(params).promise()
}
