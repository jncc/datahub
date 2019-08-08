# datahub.ingester

Ingests a single record from Topcat (with validation?) and pushes it into a DynamoDB table and onto an SQS queue for ingestion into the [elasticsearch-lambda-ingester](https://github.com/jncc/jncc-website-search/tree/master/elasticsearch-lambda-ingester) function. The record may contain a link to a pdf (or other selected ingestable format) and base64 encode it or the base64 encoded data.

## Install

Clone this repository.

```
cd /path/to/repo/Datahub.Ingester
npm install
```

### Localstack setup

In the `./localstack/` directory there is a small node project that can help setting up a localstack environment to test against with this project, its defaults are set as they are in the example events so you should just need to create the stack and then run the events against it for all to work (you will also need to set the appropriate variables in the `.env` file).

Bring up localstack (docker required)

```
cd ./localstack
npm run up
```

You will need to wait a while for the services to become active

Other operations

```
npm run create-stack    # Sets up the running localstack with S3, SQS, Elasticsearch and Dynamo
npm run delete-stack    # Deletes all parts of the localstack
npm run reset-stack     # Resets the stack to an empty state (i.e. equivalent to delete-stack => create-stack)
```

These are all running a setup.js node application, which has further commands that can be used, for more information;

```
node setup.js --help
```

### Elasticsearch

To insert some test data into elasticsearch (equivalent to the test events) to test deletes use the following from `./localstack` directory

```
npm run test:dummy
```

## Usage

There are 4 available commands to use on this template. For more info and usage descriptions, see the [node-lambda](https://github.com/motdotla/node-lambda) repository.

```
cd /path/to/repo/Datahub.Ingester
npm run setup # setup node-lambda files
npm run test # test your event handler and check output, uses event.json file as default
npm run test -- -j ./event.example.publish.json # test your event handler and check output with the ./event.example.publish.json event
# npm run package # just generate the zip that would be uploaded to AWS # NOT SETUP ATM
# npm run deploy # deploy to AWS # NOT SETUP ATM
```

## Data format

Messages will be JSON objects of the form;

```json
{
    "config": {
        "elasticsearch": {
            "index": "string",
            "site": "string"
        },
        "hub": {
            "baseUrl": "url"
        },
        "dynamo": {
            "table": "string"
        },
        "sqs": {
            "queueEndpoint": "url",
            "largeMessageBucket": "string"
        },        
        "action": "publish|unpublish|reindex"
    },
    "asset": {
        "id": "uuid",
        "metadata": {
            "title": "string",
            "abstract": "string",
            "topicCategory": "string",
            "keywords": [
                {
                    "vocab": "string",
                    "value": "string",
                    "link": "string"
                },
            ],
            "temporalExtent": {
                "begin": "string",
                "end": "string"
            },
            "datasetReferenceDate": "string",
            "lineage": "string",
            "additionalInformationSource": "string",
            "responsibleOrganisation": {
                "name": "string",
                "email": "string",
                "role": "string"
            },
            "limitationsOnPublicAccess": "string",
            "useConstraints": "string",
            "copyright": "string",
            "spatialReferenceSystem": "string",
            "metadataDate": "string",
            "metadataPointOfContact": {
                "name": "string",
                "email": "string",
                "role": "string"
            },
            "resourceType": "string",
            "boundingBox": {
                "north": "decimal",
                "south": "decimal",
                "east": "decimal",
                "west": "decimal"
            }   
        },
        "digitalObjectIdentifier": "doi-string",
        "citation": "string",    
        "image": {
            "url": "string",
            "width": "int",
            "height": "int",
            "crops": {
                "squareUrl": "string",
                "thumbnailUrl": "string"
            }
        },
        "data": [
            {
                "title": "string",
                "http": {
                    "url": "string",
                    "fileExtension": "string",
                    "fileBytes": "int"
                }
            },
            {
                "title": "string",
                "http": {
                    "url": "string",
                    "fileExtension": "string",
                    "fileBytes": "int",                
                    "fileBase64": "string"
                }
            },
        ]
    }
}
```