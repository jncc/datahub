using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Datahub.Web.Models;
using Datahub.Web.Pages.Helpers;

namespace Datahub.Web.Search
{
    public interface IElasticsearchService
    {
        ElasticClient Client();
    }

    public class ElasticsearchService : IElasticsearchService
    {
        readonly ElasticClient _client;

        // the datahub only ever searches over the "datahub" site
        static readonly string ES_SITE = "datahub";

        /// <summary>
        /// Initialises a new ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService()
        {
            var builder = new UriBuilder();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ES_ENDPOINT_SCHEME"))) 
            {
                builder.Scheme = Environment.GetEnvironmentVariable("ES_ENDPOINT_SCHEME");
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ES_ENDPOINT_PORT")))
            {
                builder.Port = int.Parse(Environment.GetEnvironmentVariable("ES_ENDPOINT_PORT"));
            }
            builder.Host = Environment.GetEnvironmentVariable("ES_ENDPOINT_HOST");

            var endpointUri = new Uri(builder.ToString());
            var pool = new SingleNodeConnectionPool(endpointUri);

            string awsAccessKey = Environment.GetEnvironmentVariable("ES_AWS_ACCESSKEY");
            string awsSecretAccessKey = Environment.GetEnvironmentVariable("ES_AWS_SECRETACCESSKEY");
            string awsRegion = Environment.GetEnvironmentVariable("ES_AWS_REGION");
            string awsProfile = Environment.GetEnvironmentVariable("ES_AWS_PROFILE");            

            if (endpointUri.IsLoopback)
            {
                // support localhost for local development
                _client = new ElasticClient(new ConnectionSettings(pool));
            }
            else if (!string.IsNullOrWhiteSpace(awsAccessKey) && !string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                // use AWS access keys to configure
                var httpConnection = new AwsHttpConnection(awsRegion, new StaticCredentialsProvider(new AwsCredentials
                {
                    AccessKey = awsAccessKey,
                    SecretKey = awsSecretAccessKey
                }));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
            else if (!string.IsNullOrWhiteSpace(awsProfile))
            {
                // use AWS profile
                throw new NotImplementedException("AWS profile support is not working yet. Specify access keys instead.");
                // var httpConnection = new AwsHttpConnection(awsRegion, new NamedProfileCredentialProvider(awsProfile));
                // _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
            else
            {
                // attempt to use AWS instance profile (for production)
                var httpConnection = new AwsHttpConnection(awsRegion, new InstanceProfileCredentialProvider());
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
        }

        public ElasticClient Client()
        {
            return _client;
        }

        public static int GetStartFromPage(int page, int size)
        {
            return (page - 1) * size;
        }

        public static QueryContainer BuildDatahubQuery(string q, List<Keyword> keywords)
        {
            QueryContainer container = null;

            // site
            container &= new MatchQuery { Field = "site", Query = ES_SITE };

            // text
            if (q.IsNotBlank())
            {
                container &= new CommonTermsQuery()
                {
                    Field = "content",
                    Query = q,
                    CutoffFrequency = 0.001,
                    LowFrequencyOperator = Operator.Or
                };
            }

            // keywords
            if (keywords.Any())
            {
                // for each keyword add a new query container containing a must match pair
                foreach (Keyword keyword in keywords)
                {
                    container &= new BoolQuery
                    {
                        Must = new QueryContainer[]
                        {   
                            new MatchQuery { Field = "keywords.vocab", Query = keyword.Vocab },
                            new MatchQuery { Field = "keywords.value", Query = keyword.Value },
                        }
                    };
                }
            }

            return container;
        }
    }
}
