const env = require('../env')
const AWS = require('aws-sdk')

const sts = new AWS.STS()

module.exports.deleteByAssetId = async function (id, index) {
  var errors = []

  // Assume role from jncc-website-x to do the invoke
  var stsParams = {
    RoleArn: env.SEARCH_DELETER_ROLE,
    RoleSessionName: 'SearchDeleterRoleSession'
  }

  console.log(`Assuming role for lambda invocation`)
  var assumedRole = await sts.assumeRole(stsParams).catch((error) => {
    console.error(`Role assumption failed: ${error}`)
    errors.push(error)
  })
  
  var tempCredentials = {
    accessKeyId: assumedRole.Credentials.AccessKeyId,
    secretAccessKey: assumedRole.Credentials.SecretAccessKey,
    sessionToken: assumedRole.Credentials.SessionToken,
  }

  var lambda = new AWS.Lambda(tempCredentials)

  var lambdaParams = {
    FunctionName: env.SEARCH_DELETER_LAMBDA,
    InvocationType: 'RequestResponse',
    LogType: 'Tail',
    Payload: `{ "id": "${id}", "index": "${index}" }`
  }

  console.log(`Invoking lambda ${env.SEARCH_DELETER_LAMBDA}`)
  lambda.invoke(lambdaParams, function(err, data) {
    if (err) {
      console.error(`${env.SEARCH_DELETER_LAMBDA} lambda delete by asset ID '${id}' request failed: ${err}`)
      errors.push(err)
    }
  })

  return { success: errors.length === 0, messages: errors }
}
