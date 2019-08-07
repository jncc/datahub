const axios = require('axios')
const urljoin = require('url-join')
const uuid4 = require('uuid/v4')

/**
 * Creates the SQS messages for a provided message object
 */
module.exports.createSQSMessages = async function (message) {
  errors = []
  var messages = [createSQSMessageForAsset(message)]

  await message.asset.data.forEach(async (resource) => {
    var { success: success, sqsMessage: sqsMessage, error: error } = await createSQSMessageForResource(message, resource)
    if (success) {
      messages.push(sqsMessage)
    } else {
      errors.push(error)
    }
  })

  if (errors.length === 0) {
    return { success: true, sqsMessages: messages }
  }
  return { success: false, errors: errors }
}

/**
 * Create a link to where the asset will live on the hub.
 *
 * @param {uuid} id The id of the asset
 * @param {url} baseUrl The base URL of the hub, provided by the config object in the inital message
 */
function getHubUrlById (id, baseUrl) {
  return urljoin(baseUrl, 'assets', id)
}

/**
 * Create a series of SQS messages that represent the indexable part of the asset
 * provided in the initial message.
 *
 * @param {message} message The initial message passed to the lambda function, containing config and the asset
 */
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
      url: getHubUrlById(message.asset.id, message.config.hub.baseUrl)
    }
  }
}

/**
 * Create an SQS message for a given resource as part of a given message, the message
 * provides config and a link to the parent of the resource.
 *
 * @param {message} message The initial message passed to the lambda function, containing config and the asset
 * @param {resource} resource The resource in that asset that we need to create a message for
 */
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

  if (resourceIsIndexable(resource)) {
    // If the resource is indexable, make sure the base64 filed is populated,
    // downloading the file at the provided url if necessary
    if (resource.http.fileBase64 === undefined) {
      await getBase64ForFile(resource.http.url).then((response) => {
        sqsMessage.file_base64 = Buffer.from(response.data, 'binary').toString('base64') 
      }).catch((error) => {
        return { success: false, error: error }
      })
    } else {
      sqsMessage.file_base64 = resource.http.fileBase64
    }
  } else {
    // Clear base64 if it exists on a non indexable resource
    delete sqsMessage.file_base64
  }

  return { success: true, sqsMessage: sqsMessage }
}

/**
 * If a resource is http resource and has a pdf file extension, then return true,
 * otherwise return false. This function is a bit of future proofing as we can
 * index non-pdf files we just choose not to at present.
 *
 * @param {resource} resource The resource to check
 */
function resourceIsIndexable (resource) {
  if (resource.http.fileExtension === 'pdf') {
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
function getBase64ForFile (url) {
  return axios.get(url, { responseType: 'arraybuffer' })
}
