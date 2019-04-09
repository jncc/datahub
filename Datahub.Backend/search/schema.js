
// This file contains the elastic search "mapping" (similar to a type definiton) as well
// as a factory function to create an entry of the same type.

const mapping = {
  "properties": {
    "site": { "type": "keyword" },
    "title": { "type": "text" },
    // 
    "content": { "type": "text", "term_vector": "with_positions_offsets" },
    // we don't ever return the content field because it could be massive.
    // we normally use the highlights field but a highlight search *might* not
    // return anything in the highlights field if there are no highlights.
    // so we need to show some content
    // todo: update this to 500 or whatever Topcat does.
    "content_truncated": {"type": "keyword", "index": false},
    "url": { "type": "keyword" }, // the clickthrough URL of the search result - needed when not known by convention or to avoid a DB call
    "keywords": {
      "type": "nested",
      "properties": {
        "vocab": { "type": "keyword" },
        "value": { "type": "keyword" }
      }
    },
    "resource_type": { "type": "keyword" },
    "published_date": { "type": "date" },
    "parent_id": { "type": "keyword" },
    "parent_title": { "type": "text" },
    "parent_resource_type": { "type": "keyword" },
    "file_extension": { "type": "keyword" },
    "file_bytes": { "type": "long", "index": false },
    "footprint": { "type": "geo_shape" },
    "timestamp_utc": { "type": "keyword" },
  }
}

const makeSearchDocumentFromTopcatRecord = (doc, hubUrl) => {
  return {
    'site': 'datahub',
    'title': doc.metadata.title,
    'content': doc.metadata.abstract,
    'content_truncated': doc.metadata.abstract, // pass this explicitly as the java lambda handler adds it on AWS
    'keywords': [
        {
          'vocab': 'http://vocab.jncc.gov.uk/website-vocab',
          'value': 'None'
        },
        ...doc.metadata.keywords
    ],
    'published_date': doc.metadata.datasetReferenceDate,
    'data_type': doc.metadata.resourceType,
    'url': hubUrl + 'assets/' + doc.id
  }
}

const makeSearchDocumentFromRemote = (doc, hubUrl) => {
  return {
    'site': 'datahub',
    'title': doc.metadata.title,
    'keywords': [
        {
          'vocab': 'http://vocab.jncc.gov.uk/website-vocab',
          'value': 'None'
        },
        ...doc.metadata.keywords
    ],
    'published_date': doc.metadata.datasetReferenceDate,
    'data_type': doc.metadata.resourceType,
    'url': hubUrl + 'assets/' + doc.parent_id,
    'file_extension': doc.file_extension,
    'file_size': doc.file_size,
    'file_base64': doc.file_base64,
    'parent_id': doc.parent_id,
    'parent_title': doc.parent_title
  }
}

module.exports = { mapping, makeSearchDocumentFromTopcatRecord, makeSearchDocumentFromRemote }
