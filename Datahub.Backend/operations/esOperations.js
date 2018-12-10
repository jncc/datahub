
const elasticSearchUtil = require('../utility/elasticSearchUtil')

module.exports.esauthtest = async function(req) {

  console.log('Hello from esauthtest')

  await elasticSearchUtil.sendSignedRequest({
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
