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
        readonly ElasticClient client;

        // the datahub only ever searches over the "datahub" site
        static readonly string ES_SITE = "datahub";

        /// <summary>
        /// Initialises a new NEST ElasticClient instance with support for localhost and AWS endpoints.
        /// </summary>
        public ElasticsearchService(IEnv env)
        {
            this.env = env;
            this.client = InitialiseClient();
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

        // todo move to QueryBuilder

        public static int GetStartFromPage(int page, int size)
        {
            return (page - 1) * size;
        }

        public static QueryContainer BuildDatahubQuery(string q, List<Keyword> keywords)
        {

            // site
            container &= new MatchQuery { Field = "site", Query = ES_SITE };
            QueryContainer fullTextcontainer;
            QueryContainer keywordSearch = new QueryContainer();

            // text
            if (q.IsNotBlank())
            {
                fullTextcontainer = new BoolQuery()
                {
                    Filter = new QueryContainer[]
                    {
                        new MatchQuery { Field = "site", Query = ES_SITE }
                    },
                    Should = new QueryContainer[]
                    {
                        new CommonTermsQuery() {
                            Field = "content",
                            Query = q,
                            CutoffFrequency = 0.001,
                            LowFrequencyOperator = Operator.Or
                        },
                        new CommonTermsQuery()
                        {
                            Field = "title",
                            Query = q,
                            CutoffFrequency = 0.001,
                            LowFrequencyOperator = Operator.Or
                        }
                    },
                    MinimumShouldMatch = 1
                };
            } else
            {
                // If we have no text search then make sure we are only matching on 
                // the correct site
                fullTextcontainer = new MatchQuery { Field = "site", Query = ES_SITE };
            }

            // keywords
            if (keywords.Any())
            {
                // TODO: check this logic even works for multiple queries, suspect it doesn't really 
                // for each keyword add a new query container containing a must match pair
                foreach (Keyword keyword in keywords)
                {
                    keywordSearch = keywordSearch && new BoolQuery
                    {
                        Must = new QueryContainer[]
                        {   
                            new MatchQuery { Field = "keywords.vocab", Query = keyword.Vocab },
                            new MatchQuery { Field = "keywords.value", Query = keyword.Value }
                        }
                    };
                }
            }

            return fullTextcontainer && +keywordSearch;
        }
    }
}
