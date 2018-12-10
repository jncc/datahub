
const sendRequest = require('../../sendRequest')

const deleteIndex = async () => {

  console.log('Deleting index \'main\'...')

  await sendRequest({
    method: 'DELETE', path: 'main'
  })

  console.log('Deleted index \'main\'.')
}

module.exports = deleteIndex