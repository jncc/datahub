
const ClaudiaApiBuilder = require('claudia-api-builder')

const db = require('./operations/dbOperations')

const api = new ClaudiaApiBuilder()

api.get('/hello', () => 'Hello!')

// Endpoint for inserting a Datahub asset.
api.post('/{env}/assets', db.putAsset, { authorizationType: 'AWS_IAM' })
api.get('/{env}/assets', db.scanAssets, { authorizationType: 'AWS_IAM' })
api.delete('/{env}/assets', db.deleteAsset, { authorizationType: 'AWS_IAM' })

module.exports = api
