# Datahub API

This is a simple Javascript API designed to be run as a lambda function fronted by an AWS API 
Gateway. It attempts to retrieve an entry from a DynamoDB table given a ID (UUID).

It expects a v2.0 API Gateway statment and will only respond to a `routeKey` matching `/asset/{id}`,
responding with the matching entry as a simple JSON document, of the form;

```json
{
    "metadata": {
        "lineage": "...",
        "boundingBox": {
            "west": 0,
            "east": 0,
            "south": 0,
            "north": 0
        },
        "keywords": [
            {
                "value": "...",
                "vocab": "http://vocab.jncc.gov.uk/jncc-domain"
            },
            {
                "value": "...",
                "vocab": "http://vocab.jncc.gov.uk/jncc-publication-category"
            },
            ...
        ],
        "limitationsOnPublicAccess": "No limitations",
        "dataFormat": "...",
        "topicCategory": "...",
        "abstract": "...",
        "metadataDate": "2020-02-27T10:00:02.056463Z",
        "datasetReferenceDate": "2008",
        "metadataPointOfContact": {
            "name": "Communications, JNCC",
            "email": "comms@jncc.gov.uk",
            "role": "pointOfContact"
        },
        "metadataLanguage": "English",
        "temporalExtent": {},
        "useConstraints": "Available under the Open Government Licence 3.0",
        "responsibleOrganisation": {
            "name": "Communications, JNCC",
            "email": "comms@jncc.gov.uk",
            "role": "publisher"
        },
        "resourceType": "publication"
    },
    "id": "8e18c30d-794e-4b0c-8d1c-378c3faf0e7c",
    "data": [
        {
            "title": "...",
            "http": {
                "fileBytes": 0,
                "fileExtension": "...",
                "url": "...",
                "fileBase64": null
            }
        }
    ],
    "citation": "...",
    "timestamp_utc": "2020-05-04T09:02:56.027Z"
}
```

If the ID doesn not exist in the DynamoDB table or a different `routeKey` is supplied
the function it will return a 404 with the following JSON attached;

```json
{"message":"Not Found"}
```

Currently any error should also respond as a 404 with that message.

## Packaging for deployment

To create a deployment package, we simply need to create a `.zip` file with dependencies, it has been
tested using `NodeJS v16` but it is likely to work on newer versions;

    npm install
    zip -r datahub.api.zip node_modules/ index.js package.json package-lock.json

This can be then fed to the Lambda update script to deploy it