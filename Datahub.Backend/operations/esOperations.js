
const AWS = require('aws-sdk')

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
 * Performs an AWS ElasticSearch Service (ES) request.
 */
let sendElasticSearchRequest = ({method, path, body}) => {

  // this function is essentially taken from the AWS example here:
  // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node

  // var region = 'eu-west-1'; // e.g. us-west-1
  // var domain = process.env.ES_DOMAIN;
  // var index = 'node-test';
  // var type = '_doc';
  // var id = '1';
  // var document = {
  //   "title": "Moneyball",
  //   "director": "Bennett Miller",
  //   "year": "2011"
  // }
  // console.log('Domain is ' + domain)

  // configure the http request
  let endpoint = new AWS.Endpoint(domain)
  let r = new AWS.HttpRequest(endpoint, 'eu-west-1') // todo: process.env.AWS_REGION
  r.method = method
  r.path += path // index + '/' + type + '/' + id; ... todo: why is this '+='?
  r.headers['host'] = process.env.ES_DOMAIN
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