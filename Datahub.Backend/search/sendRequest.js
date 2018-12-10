
const request = require('request-promise-native')
const sendSignedRequest = require('./sendSignedRequest')
const config = require('../config')

/**
 * Sends an ElasticSearch request, either to a local unsecured endpoint
 * or to a secured AWS ES service.
 */
const sendRequest = async ({method, path, body}) => {

  if (config.ES_ENDPOINT.startsWith('http://localhost')){
    return sendUnsignedLocalRequest({method, path, body})
  } else {
    return sendSignedRequest({method, path, body})
  }
}

let sendUnsignedLocalRequest = ({method, path, body}) => {
  return request(config.ES_ENDPOINT + path, {
    method: method,
    // headers: { 'host': 'localhost:9200' }, // don't think this is needed?
    json: body
  })
}

module.exports = sendRequest
