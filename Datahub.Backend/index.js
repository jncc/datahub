
const ClaudiaApiBuilder = require('claudia-api-builder')

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

api.post('/estest', es.estest,
  {
    authorizationType: 'AWS_IAM',
  }
)

module.exports = api
