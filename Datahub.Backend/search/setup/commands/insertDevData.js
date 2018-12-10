
const fs = require('fs')
const util = require('util')
const readFileAsync = util.promisify(fs.readFile)
const glob = util.promisify(require('glob'))
const sendRequest = require('../../sendRequest')
const schema = require('../../schema')

const insertDevData = async () => {

  console.log('Inserting dev data into index \'main\'...')

  let files = await glob('../../../Datahub.Web/Data/**/*.json')
  console.log(`Found ${files.length} files.`)
  
  // insert each of the dev records into the index
  for (let file of files) {
    
    let doc = JSON.parse(await readFileAsync(file , 'utf8'))
    let path = 'main/_doc/' + doc.id

    console.log(`Inserting ${path}...`)

    await sendRequest({
      method: 'POST',
      path: path,
      body: schema.makeDatahubIndexObjectFromTopcatRecord(doc),
    })
  }

  console.log('Inserted dev data into index \'main\'.')
}

module.exports = insertDevData
