using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Runtime;
using Amazon.SQS.ExtendedClient;
using Newtonsoft.Json;
using dotenv.net;
using System.Linq;

namespace write
{
    class Program
    {
        static void Main(string[] args)
        {
            DotEnv.Config();
            Console.WriteLine("Hello World!");

            var credentials = new BasicAWSCredentials(Env.Var.AwsAccessKey, Env.Var.AwsSecretAccessKey);
            var region = RegionEndpoint.GetBySystemName(Env.Var.AwsRegion);
            var s3 = new AmazonS3Client(credentials, region);
            var sqs = new AmazonSQSClient(credentials, region);
            var sqsExtendedClient = new AmazonSQSExtendedClient(sqs,
                new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3, Env.Var.SqsPayloadBucket)
            );

            string command = args.FirstOrDefault();

            if (command == "upsert-simple")
            {
                UpsertSimpleExample(sqsExtendedClient).GetAwaiter().GetResult();            
            }
            else if (command == "delete-simple")
            {
                DeleteExample(sqsExtendedClient).GetAwaiter().GetResult();            
            }
            else if (command == "upsert-pdf")
            {
                UpsertPdfExample(sqsExtendedClient).GetAwaiter().GetResult();            
            }
            else if (command == "upsert-datahub-asset")
            {
                UpsertDatahubAssetExample(sqsExtendedClient).GetAwaiter().GetResult();            
            }
            else
            {
                throw new Exception("Please specify command (see code / readme).");
            }
        }

        static async Task UpsertSimpleExample(AmazonSQSExtendedClient client)
        {
            // index a simple document
            
            var simpleMessage = new
            {
                verb = "upsert",
                index = "dev",
                document = new
                {
                    id = "123456789",
                    site = "datahub", // as opposed to website|sac|mhc
                    title = "An example searchable document.",
                    content = "This is a searchable document made purely for example purposes.",
                    url = "http://example.com/pages/123456789", // the URL of the page, for clicking through
                    keywords = new []
                    {
                        new { vocab = "http://vocab.jncc.gov.uk/jncc-web", value = "Example" }
                    },
                    published_date = "2019-01-14",
                }
            };

            var basicResponse = await client.SendMessageAsync(Env.Var.SqsEndpoint,
                JsonConvert.SerializeObject(simpleMessage, Formatting.None)
            );

            Console.WriteLine(basicResponse.MessageId);
        }

        static async Task DeleteExample(AmazonSQSExtendedClient client)
        {
            var deleteMessage = new
            {
                verb = "delete",
                index = "test",
                document = new
                {
                    id = "123456789",
                    site = "website",
                }
            };

            var deleteResponse = await client.SendMessageAsync(Env.Var.SqsEndpoint,
                JsonConvert.SerializeObject(deleteMessage, Formatting.None)
            );

            Console.WriteLine(deleteResponse.MessageId);
        }

        static async Task UpsertPdfExample(AmazonSQSExtendedClient client)
        {
            // index a PDF
            
            var pdf = File.ReadAllBytes(@"../Datahub.Web/data/OffshoreBrighton_SACO_V1.0.pdf");
            var pdfEncoded = Convert.ToBase64String(pdf);

            var pdfMessage = new
            {
                verb = "upsert",
                index = "test",
                document = new
                {
                    id = "987654321",
                    site = "datahub",
                    title = "An example PDF document",
//                  content = "The content field will be set with the contents of the parsed file",
                    url = "http://example.com/downloads/987654321.pdf",
                    keywords = new []
                    {
                        new { vocab = "http://vocab.jncc.gov.uk/jncc-web", value = "Example" }
                    },
                    published_date = "2019-01-14",
                    file_base64 = pdfEncoded, // base-64 encoded file
                    file_extension = "pdf",   // when this is a downloadable
                    file_bytes = "1048576",   // file such as a PDF, etc.
                }
            };

            var pdfResponse = await client.SendMessageAsync(Env.Var.SqsEndpoint,
                JsonConvert.SerializeObject(pdfMessage, Formatting.None)
            );

            Console.WriteLine(pdfResponse.MessageId);
        }

        static async Task UpsertDatahubAssetExample(AmazonSQSExtendedClient client)
        {
            // index an asset with several large PDF resources

            var pdf = File.ReadAllBytes(@"../Datahub.Web/data/OffshoreBrighton_SACO_V1.0.pdf");
            var pdfEncoded = Convert.ToBase64String(pdf);
            
            var resources = from i in Enumerable.Range(1, 50)
                            select new
                            {
                                title = "An example searchable PDF resource #" + i,
                                url = "http://example.com/pages/123456789", // the URL of the page, for clicking through
                                keywords = new []
                                {
                                    new { vocab = "http://vocab.jncc.gov.uk/jncc-web", value = "Example" }
                                },
                                file_base64 = pdfEncoded, // base-64 encoded file
                                file_extension = "pdf",   // when this is a downloadable
                                file_bytes = "1048576",   // file such as a PDF, etc.
                                published_date = "2019-02-15",
                            };

            var bigMessage = new
            {
                verb = "upsert",
                index = "dev",
                document = new
                {
                    id = "123456789",
                    site = "datahub", // as opposed to website|sac|mhc
                    title = "An example searchable document with resources :-)",
                    content = "This is a searchable document made purely for example purposes.",
                    url = "http://example.com/pages/123456789", // the URL of the page, for clicking through
                    keywords = new []
                    {
                        new { vocab = "http://vocab.jncc.gov.uk/jncc-web", value = "Example" }
                    },
                    published_date = "2019-01-14",
                },
                resources = resources.ToArray(),
            };

            var basicResponse = await client.SendMessageAsync(Env.Var.SqsEndpoint,
                JsonConvert.SerializeObject(bigMessage, Formatting.None)
            );

            Console.WriteLine(basicResponse.MessageId);
        }
    }

    /// <summary>
    /// Provides environment variables.
    /// </summary>
    public class Env
    {    
        static Env() { } // singleton to avoid reading a variable more than once
        private static readonly Env env = new Env();

        public string AwsRegion          { get; private set; }
        public string AwsAccessKey       { get; private set; }
        public string AwsSecretAccessKey { get; private set; }
        public string SqsEndpoint        { get; private set; }
        public string SqsPayloadBucket   { get; private set; }
        
        private Env()
        {
            AwsRegion = GetVariable("AWS_REGION");
            AwsAccessKey = GetVariable("AWS_ACCESSKEY");
            AwsSecretAccessKey = GetVariable("AWS_SECRETACCESSKEY");
            SqsEndpoint= GetVariable("SQS_ENDPOINT");
            SqsPayloadBucket = GetVariable("SQS_PAYLOAD_BUCKET");
        }

        string GetVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable)
                ?? throw new Exception($"The environment variable {variable} couldn't be read. You may need to define it in your .env file.");
        }

        public static Env Var
        {
            get { return env; }
        }
    }
}
