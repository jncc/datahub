using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Datahub.Web.Config;
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
        private readonly ElasticClient _client;
        private readonly IEnv _env;

        /// <summary>
        /// Initialises a new ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService(IEnv env)
        {
            this._env = env;

            var builder = new UriBuilder();
            if (!string.IsNullOrEmpty(this._env.ES_ENDPOINT_SCHEME))
            {
                builder.Scheme = this._env.ES_ENDPOINT_SCHEME;
            }
            if (!string.IsNullOrEmpty(this._env.ES_ENDPOINT_PORT))
            {
                builder.Port = int.Parse(this._env.ES_ENDPOINT_PORT);
            }
            builder.Host = this._env.ES_ENDPOINT_HOST;

            var endpointUri = new Uri(builder.ToString());
            var pool = new SingleNodeConnectionPool(endpointUri);

            string awsAccessKey = this._env.ES_AWS_ACCESSKEY;
            string awsSecretAccessKey = this._env.ES_AWS_SECRETACCESSKEY;
            string awsRegion = this._env.ES_AWS_REGION;
            string awsProfile = this._env.ES_AWS_PROFILE;

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

        public static QueryContainer BuildDatahubQuery(string site, string q, List<Keyword> keywords)
        {
            QueryContainer container = null;

            // site
            container &= new MatchQuery { Field = "site", Query = site };

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
