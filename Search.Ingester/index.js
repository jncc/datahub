const AWS = require('aws-sdk')
const env = require('./env')

exports.handler = async function (event, context, callback) {
    var body = JSON.parse(event.Records[0].body);
    var s3Message = false;
    var s3Bucket = body.S3BucketName;
    var s3Key = body.S3Key;
    var s3Client = null;

    // Import message from S3 if message exists
    if (s3Bucket && s3Key) {
        s3Client = new AWS.S3();
        obj = await importMessageFromS3(s3Client, s3Bucket, s3Key);
        body = JSON.parse(obj.Body.toString('ascii'));
        s3Message = true;
    }
    
    if (body.verb === "upsert") {
        await putDocument(body.document, body.index)
            .then((responseBody) => {})
            .catch((error) => {
                console.log(error);
                throw new Error('Error occurred while ingesting message');
            });
    } else if (body.verb === "delete") {
        await deleteDocument(body.document, body.index)
            .then((responseBody) => {})
            .catch((error) => {
                console.log(error);
                throw new Error('Error occured while deleting document');
            })
    } else {
        callback(new Error('Unkown verb passed to ingester expected (upsert|delete) got (' + body.verb + ')'));
    }

    if (s3Message) {
        // May fail but it doesn't matter if it does or not, will be caught by
        // bucket lifecycle if delete fails, but worth monitoring at some level
        await deleteS3Message(s3Client, s3Bucket, s3Key);
    }
}

function importMessageFromS3(client, bucket, key) {
    return client.getObject({Bucket: bucket, Key: key}, function(err, data) {
        if (err) {
            console.log(err, err.stack);
            throw new Error(err);
        }
    }).promise();
}

function deleteS3Message(client, bucket, key) {
    return client.deleteObject({Bucket: bucket, Key: key}, (error, data) => {
        if (error) {
            console.log(error);
            console.log('Could not delete message in bucket (' + bucket + ') with key (' + key + ')');
        }
    }).promise();
}

function deleteDocument(document, index) {
    return sendSignedRequest('DELETE', index + '/_doc/' + document.id);
}

function validateDocument(doc) {
    if (!doc.id) {
        throw new Error('doc.id is required.');
    }
    // Non-Datahub ID's may be not a guid
    // if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(doc.id)) {
    //     throw new Error('doc.id must be a regex'); // this is a first-stab to ensure that IDs are unique
    // }
    if (!doc.site) {
        throw new Error('doc.site is required.');
    }
    if (!['website', 'mhc', 'datahub'].includes(doc.site)) {
        throw new Error('doc.site must be one of website|mhc');
    }
    if (!doc.title) {
        throw new Error('doc.title is required.');
    }
    if (!doc.content) {
        throw new Error('doc.content is required.');
    }
    if (!doc.published_date) {
        throw new Error('doc.published_date is required.');
    }
    // https://www.myintervals.com/blog/2009/05/20/iso-8601-date-validation-that-doesnt-suck/
    if (!/^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/.test(doc.published_date)) {
        throw new Error('doc.published_date must be an ISO 8601 date. (E.g., \'2018\' will do!)');
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
        new AWS.Endpoint(env.ES_PROTOCOL + '://' + env.ES_HOSTNAME),
        env.AWS_REGION
    );
    request.method = method;
    request.path += path;
    request.headers['host'] = env.ES_HOSTNAME; // setting host explicitly seems to be required by this SDK
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
                // Need to do better check for status code, currently should probably only be 200 / 201 results but //#endregion
                // not clear from elasticsearch documentation as to what it should be
                if (this.statusCode >= 200 && this.statusCode <= 300) {
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