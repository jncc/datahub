const Ajv = require('ajv')

exports.validatePublishMessage = function (message) {
  var ajv = new Ajv({ allErrors: true })
  var validate = ajv.compile(publishSchema)
  var valid = validate(message)

  if (!valid) {
    return { valid: false, errors: validate.errors }
  }

  return { valid: true, errors: null }
}

exports.validateDeleteMessage = function (message) {
  var ajv = new Ajv({ allErrors: true })
  var validate = ajv.compile(deleteSchema)
  var valid = validate(message)

  if (!valid) {
    return { valid: false, errors: validate.errors }
  }

  return { valid: true, errors: null }
}

var publishSchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  definitions: {
    config: {
      type: 'object',
      properties: {
        elasticsearch: { $ref: '#/definitions/elasticsearch' },
        hub: { $ref: '#/definitions/hub' },
        dynamo: { $ref: '#/definitions/dynamo' },
        action: { type: 'string', pattern: '^((publish)|(unpublish)|(index))$' }
      },
      required: ['elasticsearch', 'hub', 'action'],
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
    asset: {
      type: 'object',
      properties: {
        id: { type: 'string', pattern: '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' },
        metadata: { $ref: '#/definitions/metadata' },
        digitalObjectIdentifier: { type: 'string' },
        citation: { type: 'string' },
        image: { $ref: '#/definitions/image' },
        data: { type: 'array', minItems: 1, items: { $ref: '#/definitions/data' } }
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
        useContraints: { type: 'string' },
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
        fileBytes: { type: 'integer', minimum: 1 },
        fileBase64: { type: 'string' }
      },
      required: ['url', 'fileExtension', 'fileBytes'],
      additionalProperties: false
    }
  },
  type: 'object',
  properties: {
    config: { $ref: '#/definitions/config' },
    asset: { $ref: '#/definitions/asset' }
  },
  required: ['config', 'asset'],
  additionalProperties: false
}

var deleteSchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  definitions: {
    config: {
      type: 'object',
      properties: {
        elasticsearch: { $ref: '#/definitions/elasticsearch' },
        hub: { $ref: '#/definitions/hub' },
        action: { type: 'string', pattern: '^((publish)|(unpublish)|(index))$' }
      },
      required: ['elasticsearch', 'hub', 'action'],
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
    asset: {
      type: 'object',
      properties: {
        id: { type: 'string', pattern: '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' }
      },
      required: ['id'],
      additionalProperties: false
    }
  },
  type: 'object',
  properties: {
    config: { $ref: '#/definitions/config' },
    asset: { $ref: '#/definitions/asset' }
  },
  required: ['config', 'asset'],
  additionalProperties: false
}
