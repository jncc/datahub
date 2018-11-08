
const AWS = require('aws-sdk')

const config = require('./../config')

// it's unclear from the docs on the best scope or lifecycle for AWS.HttpClient;
// it probably doesn't matter here as this code is intended to run in an ephemeral
// aws lambda container, and is only used once per lambda invocation in any case
const httpClient = new AWS.HttpClient()

module.exports.esauthtest = async function(req) {

  console.log('Hello from esauthtest')

  await sendElasticSearchRequest({
    method: 'POST',
    path: 'node-test/_doc/2',
    body: {
     "title": "Moneyball",
     "director": "Bennett Miller",
     "year": "2011"
    }
  })

  return 'Done.'
}

/**
 * Performs an AWS ElasticSearch Service (ES) request, signed with the current IAM principal.
 */
let sendElasticSearchRequest = ({method, path, body}) => {

  // this function is essentially taken from the AWS example here:
  // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node

  // configure an http request
  let r = new AWS.HttpRequest(
    new AWS.Endpoint(config.ES_DOMAIN),
    config.AWS_REGION
  )
  r.method = method
  r.path += path
  r.headers['host'] = config.ES_DOMAIN
  r.headers['Content-Type'] = 'application/json'
  r.body = JSON.stringify(body)

  // sign the request with the current IAM principal
  // (this will be the executing role of the aws lambda function)
  let credentials = new AWS.EnvironmentCredentials('AWS')
  let signer = new AWS.Signers.V4(r, 'es')
  signer.addAuthorization(credentials, new Date())

  // return a promise so we can await it, rather than letting the aws lambda finish before the request completes!
  return new Promise((resolve, reject) => {
    httpClient.handleRequest(r, null,
      (response) => {
        // console.log('Success!!! ' + response.statusCode + ' ' + response.statusMessage)
        var s = ''
        response.on('data', (chunk) => { s += chunk })
        response.on('end', () => { resolve(s) })
      },
      (error) => {
        // console.log('Error!!! ' + error)
        reject(error)
      }
    )
  })
}