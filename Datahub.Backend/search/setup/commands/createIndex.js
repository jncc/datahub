
const env = require('../../../env')
const sendRequest = require('../../sendRequest')
const schema = require('../../schema')

const createIndex = async () => {
  
  // firstly, create the index itself

  console.log(`Creating index '${env.ES_INDEX}'...`)

  await sendRequest({
    method: 'PUT',
    path: env.ES_INDEX
  })

  console.log(`Created index '${env.ES_INDEX}'.`)

  // secondly, create the "mapping" that tells elastic search
  // about our index structure

  console.log(`Creating mapping for '${env.ES_INDEX}'...`)

  await sendRequest({
    method: 'PUT',
    path: `${env.ES_INDEX}/_mapping/_doc`,
    body: schema.mapping
  })

  console.log(`Created mapping for '${env.ES_INDEX}'.`)
  
  // thirdly, create an ingest pipeline that tells elastic search
  // how to extract attachment information (PDFs)

  console.log(`Creating attachment ingest pipeline...`)

  await sendRequest({
    method: 'PUT',
    path: '_ingest/pipeline/attachment',
    body: schema.attachmentPipeline
  })

  console.log(`Created attachment ingest pipeline.`)
}

module.exports = createIndex
