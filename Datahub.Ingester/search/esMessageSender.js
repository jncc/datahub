const sendRequest = require('../aws/awsSendRequest')

module.exports.deleteById = async function (id, index) {
  var errors = []

  // Delete Children
  await sendRequest({
    method: 'POST',
    path: `${index}/_delete_by_query`,
    body: {
      query: {
        match: {
          parent_id: id
        }
      }
    }
  }).catch((error) => {
    console.error(`DELETE By Parent ID '${id}' ES request failed: ${error}`)
    errors.push(error)
  })

  // Delete asset
  await sendRequest({
    method: 'DELETE',
    path: `${index}/_doc/${id}`,
    body: {}
  }).catch((error) => {
    if (error.response.status === 404) {
      console.log(`DELETE ES request for '${id}' failed: Response was 404, so ignoring error`)
    } else {
      console.error(`DELETE ES request for '${id}' failed: ${error}`)
      errors.push(error)
    }
  })

  return { success: errors.length === 0, messages: errors }
}
