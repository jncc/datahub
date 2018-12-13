
const sendSignedRequest = require('../search/sendSignedRequest')

module.exports.esauthtest = async function(req) {

  console.log('Hello from esauthtest')

  await sendSignedRequest({
    method: 'POST',
    path: 'node-test/_doc/2',
    body: {
     "title": "Moneyball",
     "director": "Bennett Miller",
     "year": "2011"
    }
  })

  return 'Done.'
}

module.exports.putDocument = async function(req) {

  console.log('Hello from putDocument')
  console.log(req.body)

  validateDocument(req.body)

  await sendSignedRequest({
    method: 'PUT',
    path: 'main/_doc/' + body.id,
    body: req.body,
  })

  return 'Done.'
}

function validateDocument(doc) {

  if (!doc.id) {
    throw 'doc.id is required.'
  }
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(doc.id)) {
    throw 'doc.id must be a regex' // this is a first-stab to ensure that IDs are unique
  }
  if (!doc.site) {
    throw 'doc.site is required.'
  }
  if (!['website', 'mhc'].includes(doc.site)) {
    throw new 'doc.site must be one of website|mhc'
  }
  if (!doc.title) {
    throw 'doc.title is required.'
  }
  if (!doc.content) {
    throw 'doc.content is required.'
  }

  // todo: more validation
}
