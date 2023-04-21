# Datahub API List

This is a simple Javascript API designed to be run as a lambda function fronted by an AWS API 
Gateway. It attempts to retrieve a list of all entries in a given table (reducing results to id, title, timestamp tuples)

It expects a v2.0 API Gateway statment and will only respond to a `routeKey` matching `/get-all`,
responding with the matching entry as a simple JSON document, of the form;

```json
{
    "count": 309,
    "items": [
        {
            "id":"f23a26d7-07ad-4291-a42d-b422dad82351",
            "title":"JNCC Report No. 546: Guidance on Assigning Benthic Biotopes using EUNIS or the Marine Habitat Classification of Britain and Ireland (Revised 2019)","timestamp":"2020-05-04T09:02:06.239Z"
        },
        {
            "id":"c6f26835-295f-4cf4-a319-9c53619a23a6",
            "title":"JNCC Report No. 670: Review of Biodiversity Data Use in the Country Nature Conservation Bodies","timestamp":"2020-11-05T16:05:14.735Z"
        },
        ...
    ]
}
```

If a different `routeKey` is supplied
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