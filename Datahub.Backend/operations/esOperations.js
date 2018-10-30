
const request = require('request-promise-native')

module.exports.estest = async function(req) {

  console.log('Hello from estest')

  let response = await request({
    uri: 'https://www.googleapis.com/urlshortener/v1/url',
    method: 'POST',
    json: {
      "longUrl": "http://www.google.com/"
    }
  })

  console.log(response.body)

  return 'Done!'
}
