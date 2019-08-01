# datahub.ingester

Ingests a single record from Topcat (with validation?) and pushes it into a DynamoDB table and onto an SQS queue for ingestion into the [elasticsearch-lambda-ingester](https://github.com/jncc/jncc-website-search/tree/master/elasticsearch-lambda-ingester) function. The record may contain a link to a pdf (or other selected ingestable format) and base64 encode it or the base64 encoded data.

## Install

Clone this repository.

```
cd /path/to/repo/Datahub.Ingester
npm install
```

## Usage

There are 4 available commands to use on this template. For more info and usage descriptions, see the [node-lambda](https://github.com/motdotla/node-lambda) repository.

```
cd /path/to/repo/Datahub.Ingester
npm run setup # setup node-lambda files
npm run test # test your event handler and check output
npm run package # just generate the zip that would be uploaded to AWS
npm run deploy # deploy to AWS
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
        "action": "publish|unpublish|index"
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