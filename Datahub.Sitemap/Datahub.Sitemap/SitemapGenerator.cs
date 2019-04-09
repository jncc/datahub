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
using System.Collections.Generic;

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
        /// <param name="parameters">The Config Object for this lambda run, parsed from the JSON input to the lambda function</param>
        /// <param name="context">The Lambda Context for this lambda run</param>
        /// <returns></returns>
        public async Task<Parameters> SitemapGeneratorHandler(Parameters parameters, ILambdaContext context)
        {
            var dynamoClient = new AmazonDynamoDBClient();

            IEnumerable<Dictionary<string, string>> items = new List<Dictionary<string, string>> { };
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            do
            {
                ScanResponse result = await GetDynamoDbScanResults(dynamoClient, parameters.Table, lastKeyEvaluated);
                IEnumerable<Dictionary<string, string>> resultItems = result.Items.Select(x =>
                    new Dictionary<string, string>
                    {
                        ["id"] = x.Single(y => y.Key == "id").Value.S,
                        ["timestamp"] = x.Single(y => y.Key == "timestamp").Value.S
                    }
                );
                items = items.Concat(resultItems);

                lastKeyEvaluated = result.LastEvaluatedKey;
            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            var xml = CreateSitemapXML(items, parameters);

            MemoryStream mStream = new MemoryStream();
            xml.Save(mStream);

            var s3Client = new AmazonS3Client();
            
            var s3Response = await SaveSitemapToS3(s3Client, parameters.Bucket, parameters.Key, mStream);

            // TODO: Do this better
            if (s3Response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3Exception(s3Response.ToString());
            }
            
            return parameters;
        }

        /// <summary>
        /// Creates a dynamo db scan, grabbing only the id and timestamp fields and an optional paging
        /// parameter (lastKeyEvaluated), returns the result as a Task to await on the other caller 
        /// side
        /// </summary>
        /// <param name="client">The DynamoDB client to use</param>
        /// <param name="table">The table to scan through</param>
        /// <param name="lastKeyEvaluated">(Optional) The last key evaluated by a preivous scan</param>
        /// <returns></returns>
        private Task<ScanResponse> GetDynamoDbScanResults(AmazonDynamoDBClient client, string table, Dictionary<string, AttributeValue> lastKeyEvaluated = null)
        {
            ScanRequest request = new ScanRequest
            {
                TableName = table,
                AttributesToGet = new List<string> { "id", "timestamp" },
                ExclusiveStartKey = lastKeyEvaluated,
                Limit = 5
            };

            return client.ScanAsync(request);
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
        private Task<PutObjectResponse> SaveSitemapToS3(AmazonS3Client client, string bucket, string key, Stream inputStream)
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
        private string GenerateAssetURL(string id, Parameters parameters)
        {
            var url = new UriBuilder
            {
                Host = parameters.Host,
                Scheme = parameters.Scheme
            }.Uri;

            if (!string.IsNullOrWhiteSpace(parameters.BasePath))
            {
                url = url.Combine(parameters.BasePath);
            }

            return url.Combine(id).ToString();
        }

        /// <summary>
        /// Create the sitemap XML file from the scanned DynamoDB assets
        /// </summary>
        /// <param name="clientRequest">The ScanResponse from the DynamoDB table scan</param>
        /// <param name="config">The Config Object for this lambda run</param>
        /// <returns></returns>
        public XDocument CreateSitemapXML(IEnumerable<Dictionary<string, string>> items, Parameters parameters)
        {
            XNamespace xmlNS = "http://www.sitemaps.org/schemas/sitemap/0.9";

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(xmlNS + "urlset",
                    from item in items
                    select
                        new XElement(xmlNS + "url",
                            new XElement(xmlNS + "loc", GenerateAssetURL(item.Single(x => x.Key == "id").Value, parameters)),
                            new XElement(xmlNS + "lastmod", GenerateAssetURL(item.Single(x => x.Key == "timestamp").Value, parameters)),
                            new XElement(xmlNS + "changefreq", "weekly"))
                )
            );
        }
    }
}
