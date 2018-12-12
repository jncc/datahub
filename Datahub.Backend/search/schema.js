
// This file contains the elastic search "mapping" (similar to a type definiton) as well
// as a factory function to create an entry of the same type.

const mapping = {
  "properties": {
    "site": { "type": "keyword" },
    "title": { "type": "text" },
    "content": { "type": "text" },
    "content_truncated": {"type": "keyword", "index": false },
    "keywords": {
      "type": "nested",
      "properties": {
        "vocab": { "type": "keyword" },
        "value": { "type": "keyword" }
      }
    },
    "published_date": { "type": "date" },
    "parent_id": { "type": "keyword" }, // clarify
    "parent_title": { "type": "text" }, // clarify
    "mime_type": { "type": "keyword" }, // why do we need this at the asset level?
    "data_type": { "type": "keyword" }, // clarify
    "footprint": { "type": "geo_shape" },
    "url": { "type": "keyword" } // is there a URL type? The URL of the search result - needed when not known by convention.
    // maybe: website object { PageID, Author? }
    // maybe: PDF document size
  }
}

const makeDatahubIndexObjectFromTopcatRecord = (doc) => {
  return {
    'site': 'datahub',
    'title': doc.metadata.title,
    'content': doc.metadata.abstract,
    'content_truncated': doc.metadata.abstract,
    'keywords': [
        {
          'vocab': 'http://vocab.jncc.gov.uk/web-vocab',
          'value': 'publication'
        },
        ...doc.metadata.keywords
    ],
    'published_date': doc.metadata.datasetReferenceDate,
//  'parent_id'
//  'parent_title'
    'data_type': doc.metadata.resourceType,
    'url': 'https://example.com/' + doc.id
  }
}

const attachmentPipeline = {
  "description": "Extracts attachment information.",
  "processors": [
    {
      "attachment": {
        "field": "data"
      }
    },
    {
      "remove": {
        "field": "data"
      }
    },
    {
      "rename": {
        "field": "attachment.content",
        "target_field": "content"
      }
    },
    {
      "rename": {
        "field": "attachment.title",
        "ignore_missing": true,
        "target_field": "title"
      }
    },
    {
      "remove": {
        "field": "attachment"
      }
    },
    {
  		"script": {
  			"lang": "painless",
  			"source": "int last = ctx._source.content.substring(0,200).lastIndexOf(\" \"); ctx._source.content_truncated = ctx._source.content.substring(0, (last > 0 ? last : 200));"
  		}
  	}
  ]
}

module.exports = { mapping, makeDatahubIndexObjectFromTopcatRecord, attachmentPipeline }
