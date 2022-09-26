import { sendRequest } from "../aws/awsSendRequest";

module.exports.deleteById = async function (id, index) {
  var errors = [];

  // Delete Children
  await sendRequest({
    method: 'DELETE_BY_PARENT_ID',
    index: index,
    id: id
  }).catch((error) => {
    console.error(`Elasticsearch - DELETE By Parent ID '${id}' ES request failed: ${error}`)
    errors.push(error)
  });

  // Delete asset
  await sendRequest({
    method: 'DELETE',
    index: index,
    id: id,
    body: {}
  }).catch((error) => {
    if (error.response.status === 404) {
      console.log(`Elasticsearch - DELETE request for '${id}' failed: Response was 404, so ignoring error`)
    } else {
      console.error(`Elasticsearch - DELETE ES request for '${id}' failed: ${error}`)
      errors.push(error)
    }
  });

  return { success: errors.length === 0, messages: errors };
}
