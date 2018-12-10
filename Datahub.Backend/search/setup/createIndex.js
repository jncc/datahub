
const fs = require('fs')
const util = require('util')

const sendRequest = require('../sendRequest')

const createIndex = async () => {
  
  // firstly, create the index (we will call it 'main')
  console.log('Creating index \'main\'...')

  await sendRequest({
    method: 'PUT', path: 'main'
  })

  // secondly, create the "mapping" that tells elastic search all about our index structure
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

//main().then(() => console.log('Done.'))

module.exports = createIndex