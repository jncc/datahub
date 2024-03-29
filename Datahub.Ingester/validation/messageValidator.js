const Ajv = require('ajv')

exports.validatePublishOrRedindexMessage = function (message) {
  var ajv = new Ajv({ allErrors: true })
  var validate = ajv.compile(publishSchema)
  var valid = validate(message)

  if (!valid) {
    return { valid: false, errors: validate.errors }
  }

  return { valid: true }
}

exports.validateS3PublishMessage = function (message) {
  var ajv = new Ajv({ allErrors: true })
  var validate = ajv.compile(s3PublishSchema)
  var valid = validate(message)

  if (!valid) {
    return { valid: false, errors: validate.errors }
  }

  return { valid: true }
}

exports.validateUnpublishMessage = function (message) {
  var ajv = new Ajv({ allErrors: true })
  var validate = ajv.compile(unpublishSchema)
  var valid = validate(message)

  if (!valid) {
    return { valid: false, errors: validate.errors }
  }

  return { valid: true }
}

const definitions = {
  config: {
    type: 'object',
    properties: {
      elasticsearch: { $ref: '#/definitions/elasticsearch' },
      hub: { $ref: '#/definitions/hub' },
      dynamo: { $ref: '#/definitions/dynamo' },
      sqs: { $ref: '#/definitions/sqs' },
      action: { type: 'string', pattern: '^((publish)|(unpublish)|(reindex))$' }
    },
    required: ['elasticsearch', 'hub', 'dynamo', 'sqs', 'action'],
    additionalProperties: false
  },
  elasticsearch: {
    type: 'object',
    properties: {
      index: { type: 'string' },
      site: { type: 'string' }
    },
    required: ['index', 'site'],
    additionalProperties: false
  },
  hub: {
    type: 'object',
    properties: {
      baseUrl: { type: 'string', format: 'uri' }
    },
    required: ['baseUrl'],
    additionalProperties: false
  },
  dynamo: {
    type: 'object',
    properties: {
      table: { type: 'string' }
    },
    required: ['table'],
    additionalProperties: false
  },
  sqs: {
    type: 'object',
    properties: {
      queueEndpoint: { type: 'string', format: 'uri' },
      largeMessageBucket: { type: 'string' }
    },
    required: ['queueEndpoint'],
    additionalProperties: false
  },
  s3: {
    type: 'object',
    properties: {
      bucketName: { type: 'string' },
      objectKey: { type: 'string' }
    },
    required: ['bucketName', 'objectKey'],
    additionalProperties: false
  },
  asset: {
    type: 'object',
    properties: {
      id: { type: 'string', pattern: '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' },
      metadata: { $ref: '#/definitions/metadata' },
      digitalObjectIdentifier: { type: 'string' },
      citation: { type: 'string' },
      image: { $ref: '#/definitions/image' },
      data: { type: 'array', items: { $ref: '#/definitions/data' } }
    },
    required: ['id', 'metadata', 'data'],
    additionalProperties: false
  },
  metadata: {
    type: 'object',
    properties: {
      title: { type: 'string' },
      abstract: { type: 'string' },
      topicCategory: { type: 'string' },
      keywords: { type: 'array', minItems: 1, items: { $ref: '#/definitions/keyword' } },
      temporalExtent: { $ref: '#/definitions/temporalExtent' },
      datasetReferenceDate: { type: 'string' },
      lineage: { type: 'string' },
      dataFormat: { type: 'string' },
      responsibleOrganisation: { $ref: '#/definitions/organisation' },
      limitationsOnPublicAccess: { type: 'string' },
      useConstraints: { type: 'string' },
      spatialReferenceSystem: { type: 'string' },
      metadataDate: { type: 'string' },
      metadataPointOfContact: { $ref: '#/definitions/organisation' },
      resourceType: { type: 'string' },
      metadataLanguage: { type: 'string' },
      boundingBox: { $ref: '#/definitions/boundingBox' }
    },
    required: ['title', 'topicCategory', 'resourceType', 'keywords', 'abstract', 'responsibleOrganisation', 'metadataPointOfContact'],
    additionalProperties: false
  },
  keyword: {
    type: 'object',
    properties: {
      vocab: { type: 'string' },
      value: { type: 'string' },
      link: { type: 'string' }
    },
    required: ['vocab', 'value'],
    additionalProperties: false
  },
  temporalExtent: {
    type: 'object',
    properties: {
      begin: { type: 'string' },
      end: { type: 'string' }
    },
    additionalProperties: false
  },
  organisation: {
    type: 'object',
    properties: {
      name: { type: 'string' },
      email: { type: 'string', format: 'email' },
      role: { type: 'string' }
    },
    required: ['name', 'email', 'role'],
    additionalProperties: false
  },
  boundingBox: {
    type: 'object',
    properties: {
      north: { type: 'number' },
      south: { type: 'number' },
      east: { type: 'number' },
      west: { type: 'number' }
    },
    minProperties: 4,
    additionalProperties: false
  },
  image: {
    type: 'object',
    properties: {
      url: { type: 'string' },
      width: { type: 'integer', minimum: 1 },
      height: { type: 'integer', minimum: 1 },
      crop: { $ref: '#/definitions/imageCrop' }
    },
    additionalProperties: false
  },
  imageCrop: {
    type: 'object',
    properties: {
      squareUrl: { type: 'string' },
      thumbnailUrl: { type: 'string' }
    },
    additionalProperties: false
  },
  data: {
    type: 'object',
    properties: {
      title: { type: 'string' },
      http: { $ref: '#/definitions/http' }
    },
    required: ['title', 'http'],
    additionalProperties: false
  },
  http: {
    type: 'object',
    properties: {
      url: { type: 'string', format: 'uri' },
      fileExtension: { type: 'string' },
      fileBytes: { type: 'integer' },
      fileBase64: { type: 'string' }
    },
    required: ['url'],
    additionalProperties: false
  }
}

const publishSchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  definitions: definitions,
  type: 'object',
  properties: {
    config: { $ref: '#/definitions/config' },
    asset: { $ref: '#/definitions/asset' }
  },
  required: ['config', 'asset'],
  additionalProperties: false
}

const s3PublishSchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  definitions: definitions,
  type: 'object',
  properties: {
    config: {
      type: 'object',
      properties: {
        s3: { $ref: '#/definitions/s3' },
        action: { type: 'string', pattern: 's3-publish' }
      },
      required: ['s3', 'action']
    },
    asset: {
      type: 'object',
      properties: {
        id: { type: 'string', pattern: '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' }
      },
      required: ['id']
    }
  },
  required: ['config', 'asset'],
  additionalProperties: false
}

const unpublishSchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  definitions: definitions,
  type: 'object',
  properties: {
    config: {
      type: 'object',
      properties: {
        elasticsearch: { $ref: '#/definitions/elasticsearch' },
        dynamo: { $ref: '#/definitions/dynamo' },
        action: { type: 'string', pattern: 'unpublish' }
      },
      required: ['elasticsearch', 'dynamo', 'action'],
      additionalProperties: false
    },
    asset: {
      type: 'object',
      properties: {
        id: { type: 'string', pattern: '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' }
      },
      required: ['id']
    }
  },
  required: ['config', 'asset'],
  additionalProperties: false
}
