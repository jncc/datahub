const env = require('../env')
const AWS = require('aws-sdk')

module.exports.deleteByAssetId = async function (id, index) {
  var errors = []

  var lambda = new AWS.Lambda()

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
