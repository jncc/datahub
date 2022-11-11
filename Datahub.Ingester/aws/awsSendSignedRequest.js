const opensearch = require ('@opensearch-project/opensearch') 
const awsCredentialProviderNode = require('@aws-sdk/credential-provider-node')
const aws4 = require ('aws4');
const env = require('../env');
//import { AWS_REGION, ES_ENDPOINT, AWS_PROFILE } from '../env';

// // it's unclear from the docs on the best scope or lifecycle for AWS.HttpClient;
// // it probably doesn't matter here as this code is intended to run in production in an
// // aws lambda container, and is only used once per lambda invocation
// const httpClient = new AWS.HttpClient()

const createAWSConnector = (credentials, region) => {
  class AmazonConnection extends opensearch.Connection {
    buildRequestObject(params) {
      const request = super.buildRequestObject(params);
      request.service = 'es';
      request.region = region;
      request.headers = request.headers || {};
      request.headers['host'] = request.hostname;

      return aws4.sign(request, credentials)
    }
  }
  return {
    Connection: AmazonConnection
  }
}

const getClient = async () => {
  const credentials = await awsCredentialProviderNode.defaultProvider()();
  return new opensearch.Client({
    ...createAWSConnector(credentials, env.AWS_REGION),
    node: new URL(env.ES_ENDPOINT).hostname,
  })
}

// async function indexDocument(document) {
//   var client = await getClient()
//   return client.index(document)
// }

async function deleteDocument(index, id) {
  var client = await getClient()
  return client.delete({
    index: index,
    id: id
  })
}

async function deleteByParentId(index, id) {
  var query = {
    match: {
      parent_id: id
    }

  }
  return deleteByQuery(index, query)
}

async function deleteByQuery(index, query) {
  var client = await getClient()
  return client.deleteByQuery({
    index: index,
    body: {
      query: query
    }
  })
}

/**
 * Performs an AWS ElasticSearch Service (ES) request, signed with an IAM principal.
 */
const sendSignedRequest = ({ method, index, id }) => {
  switch (method) {
    case 'DELETE':
      return deleteDocument(index, id);
    case 'DELETE_BY_PARENT_ID':
      return deleteByParentId(index, id)
    default:
      break;
  }
}
// const sendSignedRequest = ({ method, index, id }) => {
//   // this function is essentially taken from the AWS example here:
//   // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node

//   // (1) configure an http request
//   const r = new AWS.HttpRequest(
//     new AWS.Endpoint(ES_ENDPOINT),
//     AWS_REGION
//   )
//   r.method = method
//   r.path += path
//   r.headers['host'] = new URL(ES_ENDPOINT).hostname // setting host explicitly seems to be required by this SDK
//   r.headers['Content-Type'] = 'application/json'
//   r.body = JSON.stringify(body)

//   // (2) sign the request for AWS IAM
//   // use the credentials of the current AWS_PROFILE if set;
//   // otherwise use AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY
//   // (AWS Lambda makes these environment vars available)
//   const credentials = AWS_PROFILE
//     ? new AWS.SharedIniFileCredentials()
//     : new AWS.EnvironmentCredentials('AWS')
//   const signer = new AWS.Signers.V4(r, 'es') // 'es' for the aws elastic search service
//   signer.addAuthorization(credentials, new Date())

//   // (3) return a promise so we can await it
//   // (rather than letting the aws lambda finish before the request completes!)
//   return new Promise((resolve, reject) => {
//     httpClient.handleRequest(r, null,
//       (response) => {
//         var s = ''
//         response.on('data', (chunk) => { s += chunk })
//         response.on('end', () => { resolve(s) })
//       },
//       (error) => {
//         reject(error)
//       }
//     )
//   })
// }
