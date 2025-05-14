import { HUB_BASE_PATH } from '../env'
import { get } from 'axios'
import urljoin from 'url-join'

/**
 * Creates the SQS messages for a provided message object
 */
export async function createSQSMessages (message) {
  var errors = []
  var messages = []

  if (message.asset.data && message.asset.data.length > 0) {
    for (var id in message.asset.data) {
      var resource = message.asset.data[id]
      if (resource.http.fileExtension && resource.http.fileBytes > 0) {
        console.log(`Creating message for file resource '${resource.title}'`)
        var { success, sqsMessage, error } = await createSQSMessageForFileResource(message, id, resource)
        if (success) {
          messages.push(sqsMessage)
        } else {
          errors.push(error)
        }
      } else {
        console.log(`Creating message for web resource '${resource.title}'`)
        messages.push(createSQSMessageForWebResource(message, id, resource))
      }
    }
  } else {
    console.log(`Creating message for asset with no resources`)
    messages.push(createSQSMessageForAssetWithNoResources(message))
  }

  if (errors.length === 0) {
    return { success: true, sqsMessages: messages }
  }
  return { success: false, errors: errors }
}

/**
 * Create a link to where the asset will live on the hub.
 *
 * @param {uuid} id The id of the asset
 * @param {string} basePath The base path of the hub, provided by the config object for this lambda
 * @param {url} baseUrl The base URL of the hub, provided by the config object in the inital message
 */
function getHubUrlFromId (baseUrl, basePath, id) {
  return urljoin(baseUrl, basePath, id)
}

/**
 * Create a link to hub asset with an anchor to the file resource.
 *
 * @param {url} baseUrl The base URL of the hub, provided by the config object in the inital message
 * @param {string} basePath The base path of the hub, provided by the config object for this lambda
 * @param {uuid} id The id of the asset
 * @param {url} fileUrl The url of the file resource
 */
function getHubResourceUrl (baseUrl, basePath, id, fileUrl) {
  const filenameAnchor = '#' + fileUrl.split('/').pop()
  return urljoin(baseUrl, basePath, id, filenameAnchor)
}

/**
 * Create an SQS message for an asset page that has no resources to index.
 *
 * @param {message} message The initial message passed to the lambda function, containing config and the asset
 */
function createSQSMessageForAssetWithNoResources (message) {
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
      url: getHubUrlFromId(message.config.hub.baseUrl, HUB_BASE_PATH, message.asset.id),
      asset_id: message.asset.id,
      file_bytes: 0
    }
  }
}

/**
 * Create an SQS message for a web resource.
 *
 * @param {message} message The initial message passed to the lambda function, containing config and the asset
 * @param {resource} resource The resource in that asset that we need to create a message for
 */
function createSQSMessageForWebResource (message, resourceIndex, resource) {
  return {
    index: message.config.elasticsearch.index,
    verb: 'upsert',
    document: {
      id: message.asset.id + '+' + resourceIndex,
      site: message.config.elasticsearch.site,
      title: resource.title,
      keywords: message.asset.metadata.keywords,
      content: message.asset.metadata.abstract,
      resource_type: message.asset.metadata.resourceType,
      published_date: message.asset.metadata.datasetReferenceDate,
      url: getHubUrlFromId(message.config.hub.baseUrl, HUB_BASE_PATH, message.asset.id),
      asset_id: message.asset.id,
      file_bytes: 0
    }
  }
}

/**
 * Create an SQS message for a file resource which relates back to the asset by using an anchor link.
 * e.g. https://hub.jncc.gov.uk/assets/99690728-aafd-4b44-ab22-31847e2184bc#Chile-Viticulture-mapping-layers.zip
 *
 * @param {message} message The initial message passed to the lambda function, containing config and the asset
 * @param {resource} resource The resource in that asset that we need to create a message for
 */
async function createSQSMessageForFileResource (message, resourceIndex, resource) {
  var sqsMessage = {
    index: message.config.elasticsearch.index,
    verb: 'upsert',
    document: {
      id: message.asset.id + '+' + resourceIndex,
      site: message.config.elasticsearch.site,
      title: resource.title,
      keywords: message.asset.metadata.keywords,
      content: message.asset.metadata.abstract,
      resource_type: message.asset.metadata.resourceType,
      published_date: message.asset.metadata.datasetReferenceDate,
      url: getHubResourceUrl(message.config.hub.baseUrl, HUB_BASE_PATH, message.asset.id, resource.http.url),
      asset_id: message.asset.id,
      file_base64: resource.http.fileBase64,
      file_bytes: resource.http.fileBytes,
      file_extension: resource.http.fileExtension
    }
  }

  return { success: true, sqsMessage: sqsMessage }
}

/**
 * If the file extension is a pdf then return true, otherwise return false.
 * This function is a bit of future proofing as we can index non-pdf files
 * we just choose not to at present.
 *
 * @param {fileExtension} fileExtension The file extension to check
 */
export function fileTypeIsIndexable (fileExtension) {
  if (fileExtension === 'pdf') {
    return true
  }
  return false
}

/**
 * Download the file at a given url and convert it into a base64 encoded string,
 * on an error use the callback function to end the lambda execution.
 *
 * @param {string} url The url to try and fetch
 */
export function getBase64ForFile (url) {
  return get(url, { responseType: 'arraybuffer' })
}
