
const url = require('url')
const AWS = require('aws-sdk')
const config = require('../config')

// it's unclear from the docs on the best scope or lifecycle for AWS.HttpClient;
// it probably doesn't matter here as this code is intended to run in production in an
// aws lambda container, and is only used once per lambda invocation
const httpClient = new AWS.HttpClient()

/**
 * Performs an AWS ElasticSearch Service (ES) request, signed with the current IAM principal.
 */
const sendSignedRequest = ({method, path, body}) => {

  // this function is essentially taken from the AWS example here:
  // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node
  
  // (1) configure an http request
  let r = new AWS.HttpRequest(
    new AWS.Endpoint(config.ES_ENDPOINT),
    config.AWS_REGION
  )
  r.method = method
  r.path += path
  r.headers['host'] = url.parse(config.ES_ENDPOINT).hostname // setting host explicitly seems to be required
  r.headers['Content-Type'] = 'application/json'
  r.body = JSON.stringify(body)
  
  // (2) sign the request for AWS IAM
  let credentials = config.AWS_ACCESS_KEY
    ? new AWS.Credentials(config.AWS_ACCESS_KEY, config.AWS_SECRET_KEY) // sign the request with the configured IAM key
    : new AWS.EnvironmentCredentials('AWS') // use the executing role of the aws lambda function
  let signer = new AWS.Signers.V4(r, 'es') // 'es' for the aws elastic search service
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
