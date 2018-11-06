
const request = require('request-promise-native')


let main = async function () {

  let query1 = {
    '_source': ['title'], // only return these fields
    'query': { 'match_all': {} }, // match all entries
  }
  let response0 = await doQuery(query1)
  console.log(`First hit is ${JSON.stringify(response0.hits.hits[0]._source)}`)
  
  let query2 = {
    '_source': ['title'],
    "query": { "match": { "title": "monitoring" } }
  }
  await doQuery(query2)
}

let doQuery = async (query) => {  
  let response = await request('http://localhost:9200/main/_search', {
    method: 'GET',
    json: query
  })
  console.log(`Found ${response.hits.total} hits.`)
  return response
}

main()
