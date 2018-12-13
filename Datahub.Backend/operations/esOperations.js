
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
    path: 'main/_doc/' + req.body.id,
    body: req.body,
  })

  return 'Done.'
}

function validateDocument(doc) {

  if (!doc.id) {
    throw 'doc.id is required.'
  }
  if (! /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(doc.id)) {
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
  if (!doc.published_date) {
    throw 'doc.published_date is required.'
  }
  // https://www.myintervals.com/blog/2009/05/20/iso-8601-date-validation-that-doesnt-suck/
  if (! /^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/.test(doc.published_date)) {
    throw 'doc.published_date must be an ISO 8601 date. (E.g., \'2018\' will do!)'
  }

  // todo: more validation
}
