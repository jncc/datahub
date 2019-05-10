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
        readonly Env env;
        readonly ElasticClient client;

        /// <summary>
        /// Initialises a new NEST ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService(Env env)
        {
            this.env = env;
            client = InitialiseClient();
        }

        ElasticClient InitialiseClient()
        {
            // build the endpoint URL from its components to workaround apparent issue
            // with putting full URLs in AWS environment variables
            var b = new UriBuilder
            {
                Scheme = env.ES_ENDPOINT_SCHEME,
                Host = env.ES_ENDPOINT_HOST,
                Port = Convert.ToInt32(env.ES_ENDPOINT_PORT),
            };

            var endpoint = new Uri(b.ToString());
            var pool = new SingleNodeConnectionPool(endpoint);

            if (endpoint.IsLoopback)
            {
                // support localhost for local development
                return new ElasticClient(new ConnectionSettings(pool));
            }
            else if (env.AWS_ACCESS_KEY_ID.IsNotBlank() && env.AWS_SECRET_ACCESS_KEY.IsNotBlank())
            {
                var connection = new AwsHttpConnection(env.AWS_DEFAULT_REGION,
                    new StaticCredentialsProvider(
                        new AwsCredentials
                        {
                            AccessKey = env.AWS_ACCESS_KEY_ID,
                            SecretKey = env.AWS_SECRET_ACCESS_KEY,
                        }));

                return new ElasticClient(new ConnectionSettings(pool, connection));
            }
            else
            {
                // attempt to use AWS instance profile (for production)
                var connection = new AwsHttpConnection(env.AWS_DEFAULT_REGION, new InstanceProfileCredentialProvider());
                return new ElasticClient(new ConnectionSettings(pool, connection));
            }
        }

        public ElasticClient Client()
        {
            return client;
        }
    }
}
