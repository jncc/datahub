
Under development
-----------------

This is a Node v8 project that implements an AWS Lambda handler that upsert records into the Datahub.

Development
-----------
You just need Node v8 and Yarn. Restore packages by running 

    yarn


Deployment
----------

Claudia.js is used to deploy the code to AWS. Just run

    yarn deploy

Usage
-----

The root API URL is in the claudia.json file. There is a sanity test API, open to the world:

    GET https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/hello

The other APIs are secured with AWS IAM. You might use Postman to test these APIs. For example:

    POST https://bz79yqmu63.execute-api.eu-west-1.amazonaws.com/latest/assets

Pass the asset in the request body. (Note: if you forget to specify the content type, the server won't parse JSON.)

    Body: { "asset": { "id": "someID", ... } }
    Content-Type: application/json
    

 