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
    public class AssetInfo
    {
        public string Id { get; set; }
        public string Timestamp { get; set; }
    }

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
            var items = (new List<AssetInfo> { }).AsEnumerable();

            // Loop through pages of results returned by dynamodb, lastKeyEvaluated is populated by the returned
            // query and is passed into the next query as the pointer to the start of the next page
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            do
            {
                var result = await GetDynamoDbScanResults(dynamoClient, parameters.Table, lastKeyEvaluated);
                var resultItems = result.Items.Select(x =>
                    new AssetInfo {
                        Id = x.Single(y => y.Key == "id").Value.S,
                        Timestamp = x.Single(y => y.Key == "timestamp_utc").Value.S
                    }
                );
                items = items.Concat(resultItems);

                lastKeyEvaluated = result.LastEvaluatedKey;
            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

            var xml = CreateSitemapXML(items, parameters);

            var mStream = new MemoryStream();
            xml.Save(mStream, SaveOptions.DisableFormatting);

            var s3Client = new AmazonS3Client();
            var s3Response = await SaveSitemapToS3(s3Client, parameters.Bucket, parameters.Key, mStream);

            // TODO: Check the save response is valid and has successfully pushed, not necessarily guaranteed by
            // the http status code in all cases (need to check size/etag[md5]/etc...)
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
            var request = new ScanRequest
            {
                TableName = table,
                AttributesToGet = new List<string> { "id", "timestamp_utc" },
                ExclusiveStartKey = lastKeyEvaluated
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
            return client.PutObjectAsync(new PutObjectRequest
            {
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
        /// <param name="scheme">The Scheme to use with the host</param>
        /// <param name="host">The Host URL to base this sitemap on</param>
        /// <param name="basePath">An Optional basePath parameter to inlcude with the objects</param>
        /// <returns></returns>
        private string GenerateAssetURL(string id, string scheme, string host, string basePath = null)
        {
            var url = new UriBuilder
            {
                Host = host,
                Scheme = scheme
            }.Uri;

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                url = url.Combine(basePath);
            }

            return url.Combine(id).ToString();
        }

        /// <summary>
        /// Create an XML element for a given asset (id/timestamp) combo, timestamp may be null so need to deal with it here
        /// </summary>
        /// <param name="xmlNS">The XML Namesapce to use</param>
        /// <param name="id">The id of the asset</param>
        /// <param name="timestamp">The timestamp of the asset</param>
        /// <param name="scheme">The Scheme to use with the host</param>
        /// <param name="host">The Host URL to base this sitemap on</param>
        /// <param name="basePath">An Optional basePath parameter to inlcude with the objects</param>        
        /// <param name="changeFreq">(Optional) The change frequency of the asset (defaults to weekly)</param>
        /// <returns></returns>
        private XElement CreateElement(XNamespace xmlNS, string id, string timestamp, string scheme, string host, string basePath = null, string changeFreq = "weekly")
        {
            return new XElement(xmlNS + "url",
                    new XElement(xmlNS + "loc", GenerateAssetURL(id, scheme, host, basePath)),
                    new XElement(xmlNS + "lastmod", timestamp),
                    new XElement(xmlNS + "changefreq", changeFreq));
        }

        /// <summary>
        /// Create the sitemap XML file from the scanned DynamoDB assets
        /// </summary>
        /// <param name="clientRequest">The ScanResponse from the DynamoDB table scan</param>
        /// <param name="config">The Config Object for this lambda run</param>
        /// <returns></returns>
        public XDocument CreateSitemapXML(IEnumerable<AssetInfo> items, Parameters parameters)
        {
            XNamespace xmlNS = "http://www.sitemaps.org/schemas/sitemap/0.9";

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(xmlNS + "urlset",
                    from item in items
                    select
                    CreateElement(xmlNS, item.Id, item.Timestamp, parameters.Scheme, parameters.Host, parameters.BasePath, parameters.ChangeFrequency)
                )
            );
        }
    }
}
