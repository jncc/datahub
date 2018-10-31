
const request = require('request-promise-native')
const AWS = require('aws-sdk')

module.exports.estest = async function(req) {

  console.log('Hello from estest')

  let response = await request({
    uri: 'process.env.ES_DOMAIN/movies/movie',
    method: 'POST',
    json: {"title": "My Neighbour Totoro"}
  })

  console.log(response.body)

  return 'Done!'
}

module.exports.esauthtest = async function(req) {

  console.log('Hello from esauthtest')

  await makeElasticSearchRequest({
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
 * Makes an AWS ElasticSearch Service (ES) request.
 */
let makeElasticSearchRequest = ({method, path, body}) => {

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


  let endpoint = new AWS.Endpoint(domain)
  let r = new AWS.HttpRequest(endpoint, 'eu-west-1') // todo: process.env.AWS_REGION

  r.method = method
  r.path += path // index + '/' + type + '/' + id; ... todo: why is this '+='?
  r.headers['host'] = process.env.ES_DOMAIN
  r.headers['Content-Type'] = 'application/json'
  r.body = JSON.stringify(body)

  // sign the request with the current IAM principal
  let credentials = new AWS.EnvironmentCredentials('AWS')
  let signer = new AWS.Signers.V4(r, 'es')
  signer.addAuthorization(credentials, new Date())

  let httpClient = new AWS.HttpClient()

  // return a promise so we can await it, rather than letting the aws lambda just finish!
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