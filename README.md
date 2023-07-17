# JNCC Datahub

This is the JNCC ~~Datahub~~ Resourcehub app.

It contains the following projects
- Datahub.Api (Node.js AWS lambda function for retrieving single assets from DynamoDB)
- Datahub.Api.GetAll (Node.js AWS lambda function for retrieving a list of all assets from DynamoDB)
- Datahub.Web (ASP.NET Core web front-end)
- Datahub.Ingester (Node.js AWS Lambda handler for writing to DynamoDB and Elasticsearch)
- Datahub.Sitemap (.NET Core AWS lambda function for generating a sitemap.xml and saving to S3)
- Datahub.Toolbox (Python scripts for Jenkins jobs)

Please see the individual readme.md files for more information.
