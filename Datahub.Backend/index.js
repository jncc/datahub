
const ClaudiaApiBuilder = require('claudia-api-builder')

const db = require('./operations/dbOperations')
const es = require('./operations/esOperations')

const api = new ClaudiaApiBuilder()

api.get('/hello', () => 'Hello!')

// Endpoint for inserting a Datahub asset.
api.post('/assets', db.putAsset, { authorizationType: 'AWS_IAM' })
api.get('/assets', db.scanAssets, { authorizationType: 'AWS_IAM' })

// Endpoint for inserting an entry into the elasticsearch index.
// This API is for use by the jncc-website and the microsites.
api.put('/search', es.putDocument, { authorizationType: 'AWS_IAM' })

module.exports = api
