
const fs = require('fs')
const util = require('util')
const readFileAsync = util.promisify(fs.readFile)
const glob = util.promisify(require('glob'))
const env = require('../../../env')
const sendRequest = require('../../sendRequest')
const schema = require('../../schema')

const insertDummyData = async () => {

  console.log(`Inserting dummy data into index '${env.ES_INDEX}'...`)

  let files = await glob('../../../Datahub.Web/Data/**/*.json')
  console.log(`Found ${files.length} files.`)
  
  // insert each of the dummy entries into the index
  for (let file of files) {
    
    let doc = JSON.parse(await readFileAsync(file , 'utf8'))
    let path = env.ES_INDEX + '/_doc/' + doc.id

    console.log(`Inserting ${path}...`)

    await sendRequest({
      method: 'POST', // shouldn't this be PUT?
      path: path,
      body: schema.makeSearchDocumentFromTopcatRecord(doc),
    })
  }

  console.log(`Inserted dummy data into index '${env.ES_INDEX}'.`)
}

module.exports = insertDummyData
