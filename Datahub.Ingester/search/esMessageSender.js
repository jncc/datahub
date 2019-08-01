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
    errors.push(error)
  })

  // Delete asset
  await sendRequest({
    method: 'DELETE',
    path: `${index}/doc/${id}`,
    body: {}
  }).catch((error) => {
    errors.push(error)
  })

  return (errors.length === 1, errors)
}
