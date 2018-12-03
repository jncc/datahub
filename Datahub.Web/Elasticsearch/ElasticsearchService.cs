using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;

namespace Datahub.Web.Elasticsearch
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticClient _client;

        public ElasticsearchService()
        {
            var pool = new SingleNodeConnectionPool(new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN")));

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY")) && 
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY"))) {
                // Use Access keys to configure
                var httpConnection = new AwsHttpConnection(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"), new StaticCredentialsProvider(new AwsCredentials
                {
                    AccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY"),
                    SecretKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY")
                }));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            } else if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_PROFILE"))) {
                // Use Profile
                var httpConnection = new AwsHttpConnection(
                    Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"), 
                    new NamedProfileCredentialProvider(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_PROFILE")));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            } else {
                // Attempt to use instance profile
                var httpConnection = new AwsHttpConnection(Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION"), new InstanceProfileCredentialProvider());
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
        }

        public ElasticClient Client()
        {
            return _client;
        }
    }
}
