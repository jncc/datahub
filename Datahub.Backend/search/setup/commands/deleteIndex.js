
const env = require('../../../env')
const sendRequest = require('../../sendRequest')

const deleteIndex = async () => {

  console.log(`Deleting index '${env.ES_INDEX}'...`)

  await sendRequest({
    method: 'DELETE',
    path: env.ES_INDEX
  })

  console.log(`Deleted index '${env.ES_INDEX}'.`)
}

module.exports = deleteIndex