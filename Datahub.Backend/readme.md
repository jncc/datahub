# Datahub.Backend

This is a Node v8 project which implements API endpoints, handlers and setup scripts for the JNCC Datahub, as well as the JNCC Website ElasticSearch service which is used and managed by the Datahub.

## Development

You just need Node v8 and Yarn. Install packages by running

    yarn

 You can make a local ElasticSearch server easily with Docker:

    yarn search:pull      # pull Docker image
    yarn search:run       # run Docker image

N.B. On windows machines the search:run command will not work due to an issue with the $(docker build -q .) section of the command; to run in windows either navigate to ./search/dev folder and run the following command in Powershell;

    docker run -p 9200:9200 -p 9300:9300 -e \"discovery.type=single-node\" $(docker build -q .)

Or navigate to the backend folder and run the following;

    cd "./search/dev"; docker run -p 9200:9200 -p 9300:9300 -e \"discovery.type=single-node\" $(docker build -q .)

To setup the search index and dummy data, use the search setup script.

    yarn search:setup create-index --index main --endpoint http://localhost:9200/
    yarn search:setup insert-dummy-data --index main --endpoint http://localhost:9200/

This setup script can also be used (with caution) to set up indexes on the live AWS managed instance. You need to configure an appropriate AWS profile:

    aws configure --profile jncc-website-live-search-developer-writer

Then set the `AWS_REGION` and `AWS_PROFILE` environment variables before running the setup script as above. E.g.

    export AWS_REGION=eu-west-1
    export AWS_PROFILE=jncc-website-live-search-developer-writer

Alternatively you can pass these variables as optional arguments to the script.

    yarn search:setup create-index --index test --endpoint https://our.live.search.endpoint.amazonaws.com/ --aws-region eu-west-1 --aws-profile jncc-website-live-search-developer-writer

## Deployment

Claudia.js is used to deploy the code to AWS. Just run:

    yarn deploy

## Usage

You can make HTTP requests but they needed to be signed with valid IAM credentials. You can use an AWS SDK, a .NET signing package such as Aws4RequestSigner or Postman for manual requests.

If using Postman, be sure to set:

    Content-Type: application/json

Note: if you forget to specify the content type, the server won't parse JSON. You also need to use the AWS Signature authorization fields.

The root API URL is in the claudia.json file. There is a sanity test API, open to the world:

    GET https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/hello

The other APIs are secured with AWS IAM. You might use Postman to test these APIs. For example:

    POST https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/assets

Pass the asset in the request body.

## Shared Elastic Search service

There is a priliminary API endpoint to upsert entries into the shared Search service.

    PUT https://sbu241ug78.execute-api.eu-west-1.amazonaws.com/latest/search

    {
        "id": "edff0279-375b-48f5-871a-51e1a5b815ad",
        "site": "website",
        "title": "This is an example",
        "content": "This is some example content that should be indexed.",
        "keywords": [
            { "vocab": "http://vocab.jncc.gov.uk/web-vocab", "value": "Example" }
        ],
        "published_date": "2018-12-13",
        "url": "http://example.com/examples/7b1d5345-d14c-4958-a5d3-e4d8f8ba0910"
    }