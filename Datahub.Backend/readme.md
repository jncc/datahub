# Datahub.Backend

This is a Node v8 project which implements API endpoints, handlers and setup scripts for the JNCC Datahub, as well as the JNCC Website ElasticSearch service which is used and managed by the Datahub.

## Development

You just need Node v8 and Yarn. Install packages by running

    yarn

 You can make a local ElasticSearch server easily with Docker:

    yarn search:run             # run Docker image

To setup the search index and dummy data, use the search setup script.

    yarn search:setup create-index --index dev --endpoint http://localhost:9200/
    yarn search:setup insert-dummy-data --index dev --endpoint http://localhost:9200/

This setup script can also be used (with caution) to set up indexes on the live AWS managed instance. You need to configure an appropriate AWS profile:

    aws configure --profile jncc-website-live-search-developer-writer

Then set the `AWS_REGION` and `AWS_PROFILE` environment variables before running the setup script as above. E.g.

    export AWS_REGION=eu-west-1
    export AWS_PROFILE=jncc-website-live-search-developer-writer

Alternatively you can pass these variables as optional arguments to the script.

    yarn search:setup create-index --index beta --endpoint https://our.live.search.endpoint.amazonaws.com/ --aws-region eu-west-1 --aws-profile jncc-website-live-search-developer-writer

## Deployment

Claudia.js is used to deploy the code to AWS. Pass the AWS profile to use with `--profile`.

    yarn deploy [--profile jncc-claudia-deployer]

## Usage

You can make HTTP requests but they needed to be signed with valid IAM credentials. You can use an AWS SDK, a .NET signing package such as Aws4RequestSigner or Postman for manual requests.

If using Postman, be sure to set:

    Content-Type: application/json

Note: if you forget to specify the content type, the server won't parse JSON. You also need to use the AWS Signature authorization fields.

The root API URL is in the claudia.json file. There is a sanity test API, open to the world:

    GET https://someid.execute-api.eu-west-1.amazonaws.com/latest/hello

The other APIs are secured with AWS IAM. You might use Postman to test these APIs. For example:

    POST https://someid.execute-api.eu-west-1.amazonaws.com/latest/assets

Pass the asset in the request body.
