
Under development
-----------------

This is a Node v8 project that implements API endpoints and handlers for the Datahub and its Elastic Search service, which is shared by the JNCC Website.

Development
-----------
You just need Node v8 and Yarn. Restore packages by running 

    yarn

 You can make a local ElasticSearch server easily with Docker:

    yarn search:pull      # pull Docker image
    yarn search:run       # run Docker image

To setup the data and index, use the setup script:

    yarn search:setup create-index --endpoint http://localhost:9200/ --index main
    yarn search:setup insert-dev-data --endpoint http://localhost:9200/ --index main

This setup script can also be used (with caution) to setup indexes on the live instance. You will need to add AWS variables in a project .env file. See .env.example (TODO). Do not confuse this with the Datahub.Web project .env file.

Deployment
----------

Claudia.js is used to deploy the code to AWS. Just run:

    yarn deploy

Usage
-----

You can make HTTP requests but they needed to be signed with valid IAM credentials. You can use an AWS SDK, a .NET signing package such as Aws4RequestSigner or Postman for manual requests.

If using Postman, be sure to set:

    Content-Type: application/json

Note: if you forget to specify the content type, the server won't parse JSON. You also need to use the AWS Signature authorization fields.

The root API URL is in the claudia.json file. There is a sanity test API, open to the world:

    GET https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/hello

The other APIs are secured with AWS IAM. You might use Postman to test these APIs. For example:

    POST https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/assets

Pass the asset in the request body. 
    

Shared Elastic Search service
-----------------------------

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

