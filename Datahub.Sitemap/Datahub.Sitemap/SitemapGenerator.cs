using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

using UrlCombineLib;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Datahub.Sitemap
{
    public class SitemapGenerator
    {

        /// <summary>
        /// A simple function that takes a config element and produces a sitemap from a dynamodb
        /// table scan
        /// </summary>
        /// <param name="input">The Config Object for this lambda run, parsed from the JSON input to the lambda function</param>
        /// <param name="context">The Lambda Context for this lambda run</param>
        /// <returns></returns>
        public async Task<Config> SitemapGeneratorHandler(Config input, ILambdaContext context)
        {
            var dynamoClient = new AmazonDynamoDBClient();


            var request = new ScanRequest
            {
                TableName = input.Table
            };

            var clientRequest = await dynamoClient.ScanAsync(request);
            var xml = CreateSitemapXML(clientRequest, input);

            MemoryStream mStream = new MemoryStream();
            xml.Save(mStream);

            var s3Client = new AmazonS3Client();
            
            var s3Response = await SaveSitemapToS3(s3Client, input.Bucket, input.Key, mStream);

            // TODO: Do this better
            if (s3Response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3Exception(s3Response.ToString());
            }
            
            return input;
        }

        /// <summary>
        /// PUTs the outputted Sitemap XML into the given bucket at the specified key, the xml is contained in a MemoryStream
        /// for simplicity of feeding the document into the S3 Client handler
        /// </summary>
        /// <param name="client">The S3 Client to use to do the PUT</param>
        /// <param name="bucket">The bucket to PUT the file in</param>
        /// <param name="key">The key path to PUT the file on in the specified bucket</param>
        /// <param name="inputStream">A Stream containing the XML file to write out to S3 (currently a MemoryStream)</param>
        /// <returns></returns>
        public Task<PutObjectResponse> SaveSitemapToS3(AmazonS3Client client, string bucket, string key, Stream inputStream)
        {   
            return client.PutObjectAsync(new PutObjectRequest{
                BucketName = bucket,
                Key = key,
                InputStream = inputStream
            });
        }

        /// <summary>
        /// Generates an asset URL for a given asset, uses an optional BasePath and Scheme/Host from the Config object, outputs 
        /// should look like;
        /// 
        /// Scheme(http|https)://Host/[Optional:BasePath]/:id
        /// 
        /// </summary>
        /// <param name="id">The ID of the asset to create a URL for</param>
        /// <param name="config">The Config Object for this lambda run</param>
        /// <returns></returns>
        private string GenerateAssetURL(string id, Config config)
        {
            var url = new UriBuilder
            {
                Host = config.Host,
                Scheme = config.Scheme
            }.Uri;

            if (!string.IsNullOrWhiteSpace(config.BasePath))
            {
                url = url.Combine(config.BasePath);
            }

            return url.Combine(id).ToString();
        }

        /// <summary>
        /// Create the sitemap XML file from the scanned DynamoDB assets
        /// </summary>
        /// <param name="clientRequest">The ScanResponse from the DynamoDB table scan</param>
        /// <param name="config">The Config Object for this lambda run</param>
        /// <returns></returns>
        public XDocument CreateSitemapXML(ScanResponse clientRequest, Config config)
        {
            XNamespace xmlNS = "http://www.sitemaps.org/schemas/sitemap/0.9";

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(xmlNS + "urlset",
                    from item in clientRequest.Items
                    select
                        new XElement(xmlNS + "url",
                            new XElement(xmlNS + "loc", GenerateAssetURL(item.Single(x => x.Key == "id").Value.S, config)),
                            new XElement(xmlNS + "changefreq", "weekly"))
                )
            );
        }
    }
}
