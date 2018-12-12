using System;
using System.Collections.Generic;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Datahub.Web.Models;
using System.Threading.Tasks;

namespace Datahub.Web.Elasticsearch
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticClient _client;

        /// <summary>
        /// Initialises a new ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService()
        {
            var builder = new UriBuilder();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_SCHEME"))) 
            {
                builder.Scheme = Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_SCHEME");
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_PORT")))
            {
                builder.Port = int.Parse(Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_PORT"));
            }
            builder.Host = Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST");

            var endpointUri = new Uri(builder.ToString());
            var pool = new SingleNodeConnectionPool(endpointUri);

            string awsAccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY");
            string awsSecretAccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY");
            string awsRegion = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION");
            string awsProfile = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_PROFILE");            

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

        public async Task<IIndexResponse> CreateDocument(SearchDocument result)
        {
            IndexRequest<SearchDocument> doc = new IndexRequest<SearchDocument>("main", "_doc", result.Id)
            {
                Document = result
            };

            return await _client.IndexAsync(doc);
        }

        public async Task<IDeleteByQueryResponse> DeleteDocument(string search)
        {
            return await _client.DeleteByQueryAsync<SearchDocument>(d => d
                .Index("main")
                .Query(q => q.Match(m => m.Field(f=> f.Title).Query("Test Title")))
            );
        }

        public static QueryContainer BuildDatahubQuery(string query = null, List<Keyword> keywords = null, string site = null)
        {
            QueryContainer container = null;

            if (!string.IsNullOrWhiteSpace(site))
            {
                // Match on site
                container &= new MatchQuery()
                {
                    Field = "site",
                    Query = site
                };
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                container &= new CommonTermsQuery()
                {
                    Field = "content",
                    Query = query,
                    CutoffFrequency = 0.001,
                    LowFrequencyOperator = Operator.Or
                };
            }

            if (keywords != null)
            {
                // For each keyword add a new query container containing a must match pair
                foreach (Keyword keyword in keywords)
                {
                    container &= new BoolQuery()
                    {
                        Must = new QueryContainer[] { new MatchQuery() { Field = "keywords.vocab", Query = keyword.Vocab }, new MatchQuery() { Field = "keywords.value", Query = keyword.Value } }
                    };
                }
            }

            return container;
        }
    }
}
