
const url = require('url')
const AWS = require('aws-sdk')
const env = require('../env')

// it's unclear from the docs on the best scope or lifecycle for AWS.HttpClient;
// it probably doesn't matter here as this code is intended to run in production in an
// aws lambda container, and is only used once per lambda invocation
const httpClient = new AWS.HttpClient()

/**
 * Performs an AWS ElasticSearch Service (ES) request, signed with an IAM principal.
 */
const sendSignedRequest = ({method, path, body}) => {

  // this function is essentially taken from the AWS example here:
  // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node
  
  // (1) configure an http request
  let r = new AWS.HttpRequest(
    new AWS.Endpoint(env.ES_ENDPOINT),
    env.AWS_REGION
  )
  r.method = method
  r.path += path
  r.headers['host'] = url.parse(env.ES_ENDPOINT).hostname // setting host explicitly seems to be required by this SDK
  r.headers['Content-Type'] = 'application/json'
  r.body = JSON.stringify(body)
  
  // (2) sign the request for AWS IAM
  // use the credentials of the current AWS_PROFILE if set;
  // otherwise use AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY
  // (AWS Lambda makes these environment vars available)
  let credentials = env.AWS_PROFILE
    ? new AWS.SharedIniFileCredentials()
    : new AWS.EnvironmentCredentials('AWS')
  let signer = new AWS.Signers.V4(r, 'es')  // 'es' for the aws elastic search service
  signer.addAuthorization(credentials, new Date())
  
  // (3) return a promise so we can await it
  // (rather than letting the aws lambda finish before the request completes!)
  return new Promise((resolve, reject) => {
    httpClient.handleRequest(r, null,
      (response) => {
        var s = ''
        response.on('data', (chunk) => { s += chunk })
        response.on('end', () => { resolve(s) })
      },
      (error) => {
        reject(error)
      }
    )
  })
}

module.exports = sendSignedRequest
