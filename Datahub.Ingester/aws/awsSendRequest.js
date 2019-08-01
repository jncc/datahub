const axios = require('axios')
const sendSignedRequest = require('./awsSendSignedRequest')
const env = require('../env')

/**
 * Sends an ElasticSearch request, either to a local unsecured endpoint
 * or to a secured AWS ES service.
 */
const sendRequest = async ({ method, path, body }) => {
  if (env.ES_ENDPOINT.startsWith('http://localhost')) {
    return sendUnsignedLocalRequest({ method, path, body })
  } else {
    return sendSignedRequest({ method, path, body })
  }
}

const sendUnsignedLocalRequest = ({ method, path, body }) => {
  return axios({
    method: method,
    url: env.ES_ENDPOINT + path,
    data: body
  })
}

module.exports = sendRequest
