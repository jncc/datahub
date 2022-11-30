const opensearch = require ('@opensearch-project/opensearch') 
const awsCredentialProviderNode = require('@aws-sdk/credential-provider-node')
const aws4 = require ('aws4');
const env = require('../env');

/**
 * Sends an ElasticSearch request, either to a local unsecured endpoint
 * or to a secured AWS ES service.
 */


const sendRequest = async ({ method, index, id }) => {
  switch (method) {
    case 'DELETE':
      return deleteDocument(index, id);
    case 'DELETE_BY_PARENT_ID':
      return deleteByParentId(index, id)
    default:
      throw new SyntaxError(`Expected DELETE or DELETE_BY_PARENT_ID but found ${method} for ${index} and document id ${id}`);
  }
}

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
  if (env.USE_LOCALSTACK) {
    opensearchEndpoint = new URL(env.ES_ENDPOINT)
    return new opensearch.Client({
      node: `${opensearchEndpoint.hostname}:${opensearchEndpoint.port}`,
    })
  } else {
    const credentials = await awsCredentialProviderNode.defaultProvider()();
    return new opensearch.Client({
      ...createAWSConnector(credentials, env.AWS_REGION),
      node: new URL(env.ES_ENDPOINT).hostname,
    })
  }
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

module.exports = sendRequest
