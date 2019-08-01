const axios = require('axios')
const urljoin = require('url-join')
const uuid4 = require('uuid/v4')

module.exports.createSQSMessages = function (message) {
  var messages = [createSQSMessageForAsset(message)]

  for (var resource in message.asset.data) {
    messages.append(createSQSMessageForResource(message, resource))
  }

  return messages
}

function getHubUrlById (id, baseUrl) {
  return urljoin(baseUrl, 'assets', id)
}

function createSQSMessageForAsset (message) {
  return {
    index: message.config.elasticsearch.index,
    verb: 'upsert',
    document: {
      id: message.asset.id,
      site: message.config.elasticsearch.site,
      title: message.asset.metadata.title,
      keywords: message.asset.metadata.keywords,
      content: message.asset.metadata.abstract,
      resource_type: message.asset.metadata.resourceType,
      published_date: message.asset.metadata.datasetReferenceDate,
      url: getHubUrlById(message.asset.metadata.id, message.config.hub.baseUrl)
    }
  }
}

async function createSQSMessageForResource (message, resource) {
  var sqsMessage = {
    index: message.config.elasticsearch.index,
    verb: 'upsert',
    document: {
      id: uuid4(),
      site: message.config.elasticsearch.site,
      title: resource.title,
      keywords: message.asset.metadata.keywords,
      content: message.asset.metadata.abstract,
      resource_type: message.asset.metadata.resourceType,
      published_date: message.asset.metadata.datasetReferenceDate,
      url: resource.http.url,
      parent_id: message.asset.id,
      parent_title: message.asset.metadata.title,
      parent_resource_type: message.asset.metadata.resourceType,
      file_bytes: resource.http.fileBytes,
      file_extension: resource.http.fileExtension
    }
  }

  if (resource.http.fileBase64 === undefined) {
    sqsMessage.file_base64 = await getBase64ForFile(resource.http.url)
  } else {
    sqsMessage.file_base64 = resource.http.fileBase64
  }

  return sqsMessage
}

function getBase64ForFile (url, callback) {
  return axios.get(url, { responseType: 'arraybuffer' })
    .then(response =>
      Buffer.from(response.data, 'binary').toString('base64'))
    .catch(error => callback(error))
}
