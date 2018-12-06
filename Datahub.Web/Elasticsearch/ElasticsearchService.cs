using System;
using System.Collections.Generic;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Datahub.Web.Models;

namespace Datahub.Web.Elasticsearch
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticClient _client;

        public ElasticsearchService()
        {
            var pool = new SingleNodeConnectionPool(new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN")));

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY")) &&
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY")))
            {
                // Use Access keys to configure
                var httpConnection = new AwsHttpConnection(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"), new StaticCredentialsProvider(new AwsCredentials
                {
                    AccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY"),
                    SecretKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY")
                }));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
            else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_PROFILE")))
            {
                // Use Profile
                var httpConnection = new AwsHttpConnection(
                    Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"),
                    new NamedProfileCredentialProvider(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_PROFILE")));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
            else
            {
                // Attempt to use instance profile
                var httpConnection = new AwsHttpConnection(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"), new InstanceProfileCredentialProvider());
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
