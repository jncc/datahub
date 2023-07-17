# Datahub Sitemap Generator

Simple Lambda function sitemap generator for the Datahub project, creates a sitemap by scanning the DynanmoDB table for the environment and creates a `sitemap.xml` for all assets discovered in that table. The function takes in a small lambda event json payload in the form of;

```json
{
    "table": "dynamnodb-table",
    "scheme": "https|http",
    "host": "public-facing-host.com",
    "base_path": "base/path/",
    "bucket": "s3-bucket",
    "key": "path/to/sitemap.xml"
}
```

The lambda function needs to be run with an IAM role that has permissions to read the specified DynamoDB table and then write the output the indicated S3 Bucket.

# Deployment

The python code in datahub-generate-sitemap-v2.py simply needs to be copied to the lambda function of the same name

# Testing

The application can be tested locally by assuming the appropriate AWS role and calling the lambda_handler function of datahub-generate-sitemap-v2.py
passing an event object matching the above payload format.

Once the code is deployed to the lambda itself it can be tested in situ using the Test function in the AWS console with an event object in the same format.
