using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

using System.Xml.Linq;

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
        /// <param name="input"></param>
        /// <param name="context"></param>
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
            
            var s3Response = await SaveSitemapToS3(s3Client, "bucket", "key", mStream);

            // TODO: Do this better
            if (s3Response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonS3Exception(s3Response.ToString());
            }
            
            return input;
        }

        public Task<PutObjectResponse> SaveSitemapToS3(AmazonS3Client client, string bucket, string key, Stream inputStream)
        {   
            return client.PutObjectAsync(new PutObjectRequest{
                BucketName = bucket,
                Key = key,
                InputStream = inputStream
            });
        }

        private string GenerateAssetURL(string id, Config config)
        {
            return new UriBuilder
            {
                Host = config.Host,
                Scheme = config.Scheme,
                Path = id
            }.ToString();
        }

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
