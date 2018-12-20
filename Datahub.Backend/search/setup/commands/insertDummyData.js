
const fs = require('fs')
const util = require('util')
const readFileAsync = util.promisify(fs.readFile)
const glob = util.promisify(require('glob'))
const env = require('../../../env')
const sendRequest = require('../../sendRequest')
const schema = require('../../schema')

const insertDummyData = async () => {

  console.log(`Inserting dummy data into index '${env.ES_INDEX}'...`)
  
  await insertDummyDocsFromWebProject()
  await insertDummyPdfDoc()

  console.log(`Inserted dummy data into index '${env.ES_INDEX}'.`)
}

module.exports = insertDummyData

const insertDummyDocsFromWebProject = async () => {

  let files = await glob('../../../Datahub.Web/Data/**/*.json')
  console.log(`Inserting ${files.length} example files found in Datahub.Web...`)
  
  // insert each of the dummy entries into the index
  for (let file of files) {
    
    let doc = JSON.parse(await readFileAsync(file , 'utf8'))
    let path = env.ES_INDEX + '/_doc/' + doc.id //+ '?pipeline=attachment'

    console.log(`Inserting ${path}...`)

    await sendRequest({
      method: 'PUT',
      path: path,
      body: schema.makeSearchDocumentFromTopcatRecord(doc),
    })
  }
}

const insertDummyPdfDoc = async () => {

  console.log(`Inserting dummy PDF into index '${env.ES_INDEX}'...`)

  let pdfEntry = await makeDummyPdfDoc()
  let pdfPath = env.ES_INDEX + '/_doc/' + '0c693e58-9da0-4e46-8453-8518f20689f2' + '?pipeline=attachment'
  console.log(`Inserting ${pdfPath}...`)

  await sendRequest({
    method: 'PUT',
    path: pdfPath,
    body: pdfEntry,
  })
}

const makeDummyPdfDoc = async () => ({
  'data': JSON.parse(await readFileAsync('./data/pdfData.json', 'utf8')).data,
  'site': 'datahub',
  //'title': 'This is a PDF document',
  //'content': 'This PDF document is for testing the PDF indexing capabilities of ElasticSearch. I hope it works.',
  // 'content_truncated': doc.metadata.abstract,
  'keywords': [
      {
        'vocab': 'http://vocab.jncc.gov.uk/website-vocab',
        'value': 'None'
      }
  ],
  'published_date': '2018-12-20',
  'url': 'https://example.com/'
})
