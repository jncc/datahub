
const ClaudiaApiBuilder = require('claudia-api-builder')

const db = require('./operations/dbOperations')
const es = require('./operations/esOperations')

const api = new ClaudiaApiBuilder()

api.get('/hello', () => 'Hello!')

// Endpoint for inserting a Datahub asset.
api.put('/assets', db.putAsset, { authorizationType: 'AWS_IAM' })
api.get('/assets', db.scanAssets, { authorizationType: 'AWS_IAM' })

module.exports = api
