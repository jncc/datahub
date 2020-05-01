const sendRequest = require('../aws/awsSendRequest')

module.exports.deleteById = async function (id, index) {
  var errors = []

  // Delete asset
  await sendRequest({
    method: 'DELETE',
    path: `${index}/_doc/${id}`,
    body: {}
  }).catch((error) => {
    if (error.response.status === 404) {
      console.log(`Elasticsearch - DELETE request for '${id}' failed: Response was 404, so ignoring error`)
    } else {
      console.error(`Elasticsearch - DELETE ES request for '${id}' failed: ${error}`)
      errors.push(error)
    }
  })

  return { success: errors.length === 0, messages: errors }
}
