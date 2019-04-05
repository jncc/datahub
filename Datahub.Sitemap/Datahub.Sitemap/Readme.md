# Datahub Sitemap Generator

Simple Lambda function sitemap generator for the Datahub project, creates a sitemap by scanning the DynanmoDB 
table for the environment and creates a `sitemap.xml` for all assets discovered in that table. The function 
takes in a small config json input in the form of;

```json
{
	"Table": "dynamnodb-table",
	"Host": "public-facing-host.com",
	"Scheme": "https|http",
	"BasePath": "base/path/",

	"Bucket": "s3-bucket",
	"Key": "path/to/sitemap.xml"
}
```

The lambda function needs to be run with an IAM role that has permissions to read the specified DynamoDB Table
and then write the output the indicated S3 Bucket. Solution includes tests but they have not been implemented at
present.

The Lambda function handler should point to;

`Datahub.Sitemap::Datahub.Sitemap.SitemapGenerator::SitemapGeneratorHandler`

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "Datahub.Sitemap/test/Datahub.Sitemap.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "Datahub.Sitemap/src/Datahub.Sitemap"
    dotnet lambda deploy-function
```
