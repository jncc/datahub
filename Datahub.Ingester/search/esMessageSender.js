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
    console.error(`Failed to send DELETE By Parent ID ES request: ${error}`)
    errors.push(error)
  })

  // Delete asset
  await sendRequest({
    method: 'DELETE',
    path: `${index}/doc/${id}`,
    body: {}
  }).then((resp) => {
    console.log(resp)
  }).catch((error) => {
    console.error(`Failed to send DELETE ES request: ${error}`)
    errors.push(error)
  })

  return { success: errors.length === 0, messages: errors }
}
