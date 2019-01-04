const request = require('request-promise-native')
const sendSignedRequest = require('./sendSignedRequest')
const env = require('../env')

/**
 * Sends an ElasticSearch request, either to a local unsecured endpoint
 * or to a secured AWS ES service.
 */
const sendRequest = async ({method, path, body}) => {

  if (env.ES_ENDPOINT.startsWith('http://localhost')) {
    return sendUnsignedLocalRequest({method, path, body})
  } else {
    return sendSignedRequest({method, path, body})
  }
}

let sendUnsignedLocalRequest = ({method, path, body}) => {
  return request(env.ES_ENDPOINT + path, {
    method: method,
    json: body
  })
}

module.exports = sendRequest
