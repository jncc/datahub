const axios = require('axios')
const urljoin = require('url-join')

const sendSignedRequest = require('./awsSendSignedRequest').default
const env = require('../env')

/**
 * Sends an ElasticSearch request, either to a local unsecured endpoint
 * or to a secured AWS ES service.
 */
const sendRequest = async ({ method, path, body }) => {
  if (env.USE_LOCALSTACK) {
    return sendUnsignedLocalRequest({ method, path, body })
  } else {
    return sendSignedRequest({ method, path, body })
  }
}

const sendUnsignedLocalRequest = ({ method, path, body }) => {
  if (method === 'DELETE') {
    return axios.delete(urljoin(env.ES_ENDPOINT, path))
  }
  if (method === 'POST') {
    return axios.post(urljoin(env.ES_ENDPOINT, path), body)
  }
}

module.exports = sendRequest
