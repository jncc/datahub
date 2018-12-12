
const sendRequest = require('../../sendRequest')
const schema = require('../../schema')

const createIndex = async () => {
  
  // firstly, create the index itself (we'll call it 'main')
  console.log('Creating index \'main\'...')

  await sendRequest({
    method: 'PUT', path: 'main'
  })

  console.log('Created index \'main\'.')

  // secondly, create the "mapping" that tells elastic search about our index structure
  console.log('Creating mapping for \'main\'...')

  await sendRequest({
    method: 'PUT',
    path: 'main/_mapping/_doc',
    body: schema.mapping
  })

  console.log('Created mapping for index \'main\'.')
  
  // thirdly, create an ingest pipeline that tells elastic search to extract attachment information (PDFs)
  console.log('Creating ingest pipeline for index \'main\'...')

  await sendRequest({
    method: 'PUT',
    path: '_ingest/pipeline/attachment',
    body: schema.attachmentPipeline
  })

  console.log('Created ingest pipeline for index \'main\'.')
}

module.exports = createIndex
