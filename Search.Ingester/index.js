const url = require('url')
const AWS = require('aws-sdk')
const env = require('./env')

exports.handler = async function (event, context, callback) {
    var body = JSON.parse(event.Records[0].body);
    
    if (body.verb === "upsert") {
        await putDocument(body.document, body.index)
            .then((responseBody) => {})
            .catch((error) => {
                console.log(error);
                callback(new Error('Error occurred while ingesting message'));
            });
    } else if (body.verb === "delete") {
        await deleteDocument(body.document.id, body.index)
            .then((responseBody) => {})
            .catch((error) => {
                console.log(error);
                callback(new Error('Error occured while deleting document'));
            })
    } else {
        callback(new Error('Unkown verb passed to ingester expected (upsert|delete) got (' + body.verb + ')'));
    }
}

function deleteDocument(id, index) {
    return sendSignedRequest('DELETE', index + '/_doc/' + document.id);
}

function validateDocument(doc) {
    if (!doc.id) {
        throw 'doc.id is required.';
    }
    if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(doc.id)) {
        throw 'doc.id must be a regex' // this is a first-stab to ensure that IDs are unique;
    }
    if (!doc.site) {
        throw 'doc.site is required.';
    }
    if (!['website', 'mhc', 'datahub'].includes(doc.site)) {
        throw new 'doc.site must be one of website|mhc';
    }
    if (!doc.title) {
        throw 'doc.title is required.';
    }
    if (!doc.content) {
        throw 'doc.content is required.';
    }
    if (!doc.published_date) {
        throw 'doc.published_date is required.';
    }
    // https://www.myintervals.com/blog/2009/05/20/iso-8601-date-validation-that-doesnt-suck/
    if (!/^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/.test(doc.published_date)) {
        throw 'doc.published_date must be an ISO 8601 date. (E.g., \'2018\' will do!)';
    }

    // todo: more validation
}

function putDocument(document, index) {
    validateDocument(document);
    return sendSignedRequest('PUT', index + '/_doc/' + document.id + '?pipeline=attachment', document);
}

/**
 * Performs an AWS ElasticSearch Service (ES) request, signed with an IAM principal.
 */
function sendSignedRequest(method, path, body) {

    // this function is essentially taken from the AWS example here:
    // https://docs.aws.amazon.com/elasticsearch-service/latest/developerguide/es-request-signing.html#es-request-signing-node

    // (1) configure an http request
    let request = new AWS.HttpRequest(
        new AWS.Endpoint(env.ES_ENDPOINT),
        env.AWS_REGION
    );
    request.method = method;
    request.path += path;
    request.headers['host'] = url.parse(env.ES_ENDPOINT).hostname; // setting host explicitly seems to be required by this SDK
    request.headers['Content-Type'] = 'application/json';
    request.body = JSON.stringify(body);

    // (2) sign the request for AWS IAM
    // use the credentials of the current AWS_PROFILE if set;
    // otherwise use AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY
    // (AWS Lambda makes these environment vars available)
    let credentials = env.AWS_PROFILE ?
        new AWS.SharedIniFileCredentials() :
        new AWS.EnvironmentCredentials('AWS');
    let signer = new AWS.Signers.V4(request, 'es'); // 'es' for the aws elastic search service
    signer.addAuthorization(credentials, new Date());

    // (3) return a promise so we can await it
    // (rather than letting the aws lambda finish before the request completes!)
    return new Promise((resolve, reject) => {
        var client = new AWS.HttpClient();
        client.handleRequest(request, {}, function(response) {
            var responseBody = '';
            response.on('data', function (chunk) {
                responseBody += chunk;
            });
            response.on('end', function (chunk) {
                console.log(this.statusCode);
                if (this.statusCode == 200) {
                    resolve(responseBody);
                } else {
                    reject({"statusCode": this.statusCode, "statusMessage": this.statusMessage, "responseBody": responseBody});
                }
            });
        }, function(error) {
            reject(error);
        });
    });
}