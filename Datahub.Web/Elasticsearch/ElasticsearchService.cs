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
        private readonly IConfiguration _configuration;
        private readonly ElasticClient _client;

        public ElasticsearchService(IConfiguration configuration)
        {
            _configuration = configuration;

            var pool = new SingleNodeConnectionPool(new Uri(_configuration["Domain"]));

            if (!string.IsNullOrEmpty(_configuration["AWS:AccessKey"]) && !string.IsNullOrEmpty(_configuration["AWS:SecretAccessKey"])) {
                // Use Access keys to configure
                var httpConnection = new AwsHttpConnection(_configuration["AWS:Region"], new StaticCredentialsProvider(new AwsCredentials
                {
                    AccessKey = _configuration["AWS:AccessKey"],
                    SecretKey = _configuration["AWS:SecretAccessKey"]
                }));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            } else if (string.IsNullOrEmpty(_configuration["AWS:Profile"])) {
                // Use Profile
                var httpConnection = new AwsHttpConnection(_configuration["AWS:Region"], new NamedProfileCredentialProvider(_configuration["AWS:Profile"]));
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            } else {
                // Attempt to use instance profile
                var httpConnection = new AwsHttpConnection(_configuration["AWS:Region"], new InstanceProfileCredentialProvider());
                _client = new ElasticClient(new ConnectionSettings(pool, httpConnection));
            }
        }

        public ElasticClient Client()
        {
            return _client;
        }
    }
}
