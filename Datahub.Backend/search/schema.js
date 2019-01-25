
// This file contains the elastic search "mapping" (similar to a type definiton) as well
// as a factory function to create an entry of the same type.

const mapping = {
  "properties": {
    "site": { "type": "keyword" },
    "title": { "type": "text" },
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
    "published_date": { "type": "date" },
    "parent_id": { "type": "keyword" }, // clarify
    "parent_title": { "type": "text" }, // clarify
    "mime_type": { "type": "keyword" }, // why do we need this at the asset level?
    "data_type": { "type": "keyword" }, // clarify
    "footprint": { "type": "geo_shape" },
    // maybe: website object { PageID, Author? }
    // maybe: PDF document size
  }
}

const makeSearchDocumentFromTopcatRecord = (doc) => {
  return {
    'site': 'datahub',
    'title': doc.metadata.title,
    'content': doc.metadata.abstract,
    'content_truncated': doc.metadata.abstract,
    'keywords': [
        {
          'vocab': 'http://vocab.jncc.gov.uk/website-vocab',
          'value': 'None'
        },
        ...doc.metadata.keywords
    ],
    'published_date': doc.metadata.datasetReferenceDate,
    'data_type': doc.metadata.resourceType,
    'url': 'https://example.com/' + doc.id
  }
}

const makeSearchDocumentFromRemote = (doc) => {
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
    'url': 'https://example.com/' + doc.parent_id,
    'parent_id': doc.parent_id,
    'parent_title': doc.parent_title,
    'data': doc.data
  }
}

const attachmentPipeline = {
  "description": "Optionally extract attachment data field and incorporates results into the document, truncates content field into a content_truncated field which is not indexed",
  "processors": [
      {
          "attachment": {
              "field": "file_base64",
              "ignore_missing": true,
              "indexed_chars": -1
          }
      },
      {
          "set": {
            "field": "file_base64",
            "value": ""
          }
      },
      {
          "remove": {
              "field": "file_base64"
          }
      },
      {
          "script": {
            "source": "if (ctx.attachment != null && ctx.attachment.content != null) { ctx.content = ctx.attachment.content }"
          }
      },
      {
          "script": {
            "source": "if (ctx.attachment != null && ctx.attachment.title != null) { ctx.title = ctx.attachment.title }"
          }
      },
      {
          "set": {
            "field": "attachment",
            "value": ""
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
              "source": "String content = ctx.content; content = content.replace(\"\n\", \"\").trim(); int last = content.substring(0,200).lastIndexOf(\" \"); ctx.content_truncated = content.substring(0, (last > 0 ? last : (200 > content.length() ? content.length() : 200)));"
          }
      }
  ]
}

const documentPipeline = {
  "description": "Simple truncator script to truncate content to 200 characters",
  "processors": [
    {
      "script": {
          "lang": "painless",
          "source": "String content = ctx.content; content = content.replace(\"\n\", \"\").trim(); int last = content.substring(0,200).lastIndexOf(\" \"); ctx.content_truncated = content.substring(0, (last > 0 ? last : (200 > content.length() ? content.length() : 200)));"
      }
    }
  ]
}

module.exports = { mapping, makeSearchDocumentFromTopcatRecord, makeSearchDocumentFromRemote, attachmentPipeline, documentPipeline }
