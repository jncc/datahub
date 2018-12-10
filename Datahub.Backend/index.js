
const ClaudiaApiBuilder = require('claudia-api-builder')

const config = require('./config')
const db = require('./operations/dbOperations')
const es = require('./operations/esOperations')

const api = new ClaudiaApiBuilder()

api.get('/hello', () => 'Hello!')

api.post('/assets', db.putAsset,
  {
    success: 201,
    authorizationType: 'AWS_IAM',
  }
)

api.get('/assets', db.scanAssets,
  {
    authorizationType: 'AWS_IAM',
  }
)

api.post('/envtest', () => {
  console.log("ES_ENDPOINT is " + config.ES_ENDPOINT)
  return 'Done!'
})

api.post('/esauthtest', es.esauthtest,
  {
    authorizationType: 'AWS_IAM',
  }
)

module.exports = api
