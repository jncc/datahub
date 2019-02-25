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
        readonly IEnv env;
        
        ElasticClient client;

        // the datahub only ever searches over the "datahub" site
        static readonly string ES_SITE = "datahub";

        /// <summary>
        /// Initialises a new NEST ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService(IEnv env)
        {
            this.env = env;

            InitialiseClient();
        }

        void InitialiseClient()
        {
            // build the endpoint URL from its components to workaround apparent issue
            // with putting full URLs in AWS environment variables
            var b = new UriBuilder
            {
                Scheme = env.ES_ENDPOINT_SCHEME,
                Host = env.ES_ENDPOINT_HOST,
                Port = Convert.ToInt32(env.ES_ENDPOINT_PORT),
            };

            var endpointUri = new Uri(b.ToString());
            var pool = new SingleNodeConnectionPool(endpointUri);        

            if (endpointUri.IsLoopback)
            {
                // support localhost for local development
                client = new ElasticClient(new ConnectionSettings(pool));
            }
            else if (env.ES_AWS_ACCESSKEY.IsNotBlank() && env.ES_AWS_SECRETACCESSKEY.IsNotBlank())
            {
                var connection = new AwsHttpConnection(env.ES_AWS_REGION,
                    new StaticCredentialsProvider(
                        new AwsCredentials
                        {
                            AccessKey = env.ES_AWS_ACCESSKEY,
                            SecretKey = env.ES_AWS_SECRETACCESSKEY,
                        }));

                client = new ElasticClient(new ConnectionSettings(pool, connection));
            }
            else if (env.ES_AWS_PROFILE.IsNotBlank())
            {
                throw new NotImplementedException("AWS profile support is not working yet. Specify access keys instead.");
            }
            else
            {
                // attempt to use AWS instance profile (for production)
                var connection = new AwsHttpConnection(env.ES_AWS_REGION, new InstanceProfileCredentialProvider());
                
                client = new ElasticClient(new ConnectionSettings(pool, connection));
            }
        }

        public ElasticClient Client()
        {
            return client;
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
