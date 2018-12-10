
const fs = require('fs')
const util = require('util')
const readFileAsync = util.promisify(fs.readFile)
const glob = util.promisify(require('glob'))
const request = require('request-promise-native')

let main = async function() {
  console.log('Hello.')

  // on first run, we need to setup the index
  await setupIndex()

  let files = await glob('../../Datahub.Web/Data/**/*.json')
  console.log(`Found ${files.length} files.`)
  
  // insert each of the dev records into the index
  for (let file of files) {
    
    let doc = JSON.parse(await readFileAsync(file , 'utf8'))
    let path = 'main/_doc/' + doc.id

    console.log(`Inserting ${path}...`)

    await sendLocalElasticSearchRequest({
      method: 'POST',
      path: path,
      body: makeIndexObject(doc),
    })
  }
}

// let sendLocalElasticSearchRequest = ({method, path, body}) => {
//   return request('http://localhost:9200/' + path, {
//     method: method,
//     headers: { 'host': 'localhost:9200' }, // is this needed?
//     json: body
//   })
// }

let setupIndex = async () => {
  
  // first off, create the index (we will call it 'main')
  console.log('Creating index \'main\'...')
  await sendLocalElasticSearchRequest({
    method: 'PUT', path: 'main'
  })

  // then, create the "mapping" that tells elastic search all about our index structure
  console.log('Creating mapping for  \'main\'...')
  await sendLocalElasticSearchRequest({
    method: 'PUT',
    path: 'main/_mapping/_doc',
    body: {
      "properties": {
        "site": {"type": "keyword"},
        "title": {"type": "text"},
        "content": {"type": "text"},
        "keywords": {
          "type": "nested",
          "properties": {
            "vocab": {"type": "keyword"},
            "value": {"type": "keyword"}
          }
        },
        "published_date": {"type": "date"},
        "parent_id": {"type": "keyword"},
        "parent_title": {"type": "text"},
        "mime_type": {"type": "keyword"},
        "data_type": {"type": "keyword"},
        "datahub_keywords": {
          "type": "nested",
          "properties": {
            "vocab": {"type": "keyword"},
            "value": {"type": "keyword"}
          }
        },
        "footprint": {
          "type": "geo_shape"
        }
        // URL (applies to datahub too, for website serp)
        // website object { PageID, Author? }
        // PDF document size
      }
    }
  })
}

let makeIndexObject = (doc) => {
  return {
    'site': 'datahub',
    'title': doc.metadata.title,
    'content': doc.metadata.abstract,
    'keywords': [
      {
        'vocab': 'http://vocab.jncc.gov.uk/web-vocab',
        'value': 'publication'
      }
    ],
    'published_date': '2004-08-01T00:00:00Z',
    'data_type': 'publication',
    'datahub_keywords': [
      {
        'vocab': 'http://vocab.jncc.gov.uk/web-category',
        'value': 'Common Standards Monitoring'
      },
      {
        'vocab': 'http://vocab.jncc.gov.uk/jncc-publication-category',
        'value': 'Protected sites monitoring'
      },
      {
        'vocab': 'http://vocab.jncc.gov.uk/jncc-publication-category',
        'value': 'UK Habitats and Species'
      },
      {
        'vocab': 'http://vocab.jncc.gov.uk/jncc-publication-category',
        'value': 'Marine'
      },
      {
        'vocab': '',
        'value': 'CSM'
      }
    ],
  }
}

main().then(() => console.log('Done.'))
