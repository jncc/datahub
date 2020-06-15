const sendRequest = require('../aws/awsSendRequest')

module.exports.deleteByAssetId = async function (id, index) {
  var errors = []

  // Delete documents with asset id
  await sendRequest({
    method: 'POST',
    path: `${index}/_delete_by_query`,
    body: {
      query: {
        match: {
          asset_id: id
        }
      }
    }
  }).catch((error) => {
    console.error(`Elasticsearch - DELETE By Asset ID '${id}' ES request failed: ${error}`)
    errors.push(error)
  })

  return { success: errors.length === 0, messages: errors }
}
