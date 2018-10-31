
const request = require('request-promise-native')
const AWS = require('aws-sdk')

module.exports.estest = async function(req) {

  console.log('Hello from estest')

  let response = await request({
    uri: 'process.env.ES_DOMAIN/movies/movie',
    method: 'POST',
    json: {"title": "My Neighbour Totoro"}
  })

  console.log(response.body)

  return 'Done!'
}

module.exports.esauthtest = function(req) {

  console.log('Hello from esauthtest')

  var region = 'eu-west-1'; // e.g. us-west-1
  var domain = process.env.ES_DOMAIN;
  var index = 'node-test';
  var type = '_doc';
  var id = '1';
  var json = {
    "title": "Moneyball",
    "director": "Bennett Miller",
    "year": "2011"
  }
  
  indexDocument(json);
  
  function indexDocument(document) {
    var endpoint = new AWS.Endpoint(domain);
    var request = new AWS.HttpRequest(endpoint, region);
  
    request.method = 'PUT';
    request.path += index + '/' + type + '/' + id;
    request.body = JSON.stringify(document);
    request.headers['host'] = domain;
    request.headers['Content-Type'] = 'application/json';
  
    var credentials = new AWS.EnvironmentCredentials('AWS');
    var signer = new AWS.Signers.V4(request, 'es');
    signer.addAuthorization(credentials, new Date());
  
    var client = new AWS.HttpClient();
    console.log('Sending request ')

    await new Promise((resolve, reject) => {

      client.handleRequest(request, null, function(response) {
        console.log(response.statusCode + ' ' + response.statusMessage);
        var r = '';
        response.on('data', function (chunk) {
          r += chunk;
        });
        response.on('end', function (chunk) {
          console.log('Response body: ' + r);
          resolve(r)
        });
      }, function(error) {
        console.log('Error: ' + error);
        reject(error)
      });

    })    
  }

  return 'Done.'
}