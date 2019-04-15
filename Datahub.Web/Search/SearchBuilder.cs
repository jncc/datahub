
using System;
using System.Collections.Generic;
using System.Linq;
using Datahub.Web.Search;
using Datahub.Web.Models;
using Nest;
using Datahub.Web.Pages.Helpers;

namespace Datahub.Web.Search
{
    public interface ISearchBuilder
    {
        List<Keyword> ParseKeywords(string[] keywords);
        SearchDescriptor<SearchResult> BuildQuery(SearchParams input);
        QueryContainer BuildDatahubQuery(string q, List<Keyword> keywords);
    }

    public class SearchBuilder : ISearchBuilder
    {
        readonly IEnv _env;
        readonly IElasticsearchService _esService;

        // the datahub only ever searches over the "datahub" site
        static readonly string ES_SITE = "datahub";

        public SearchBuilder(IEnv env, IElasticsearchService esService)
        {
            _env = env;
            _esService = esService;
        }

        public SearchDescriptor<SearchResult> BuildQuery(SearchParams input)
        {
            return new SearchDescriptor<SearchResult>()
                .Index(_env.ES_INDEX)
                .From(GetStartFromPage(input.p, input.size))
                .Size(input.size)
                .Source(src => src
                    .IncludeAll()
                    .Excludes(e => e
                        .Field(f => f.Content)
                    )
                )
                .Query(l => BuildDatahubQuery(input.q, ParseKeywords(input.k)))
                .Highlight(h => h
                    .Fields(f => f.Field(x => x.Content)
                                    .Type(HighlighterType.Fvh)
                                    .Order(HighlighterOrder.Score)
                                    .NumberOfFragments(1),
                            f => f.Field(x => x.Title)
                    )
                    .PreTags("<b>")
                    .PostTags("</b>")
                );
        }

        public List<Keyword> ParseKeywords(string[] keywords)
        {
            return keywords.Select(k =>
            {
                int lastIndexOfSlash = k.LastIndexOf('/');
                if (lastIndexOfSlash > 0)
                {
                    // has a slash, so assume this keyword this has a vocab
                    string vocab = k.Substring(0, lastIndexOfSlash);
                    string value = k.Substring(lastIndexOfSlash + 1);
                    return new Keyword { Vocab = vocab, Value = value };
                }
                else
                {
                    return new Keyword { Vocab = null, Value = k };
                }
            }).ToList();
        }

        public int GetStartFromPage(int page, int size)
        {
            return (page - 1) * size;
        }

        public QueryContainer BuildDatahubQuery(string q, List<Keyword> keywords)
        {
            /**
             * Full Text Search Logic
             * 
             * Use a bool query, `Filter` on the site first (reduce search area), then match on
             * `Should`, entires in `Should` are searches on title and content, at least one of
             * these should match (MinimumShouldMatch = 1)
             *
             * If we have no string to search on we convert the Bool Query to a single MatchQuery
             * matching on the Site (identical to the initial `Filter` query)
             */
             
            QueryContainer fullQuery;

            List<QueryContainer> commonQueries = new List<QueryContainer>();

            if (q.IsNotBlank()) {
                commonQueries.Add(new CommonTermsQuery() {
                            Field = "content",
                            Query = q,
                            CutoffFrequency = 0.001,
                            LowFrequencyOperator = Operator.Or});
                commonQueries.Add(new CommonTermsQuery() {
                            Field = "title",
                            Query = q,
                            CutoffFrequency = 0.001,
                            LowFrequencyOperator = Operator.Or
                        });
            }

            var keywordQueries = new List<QueryContainer>();

            if (keywords.Any()) {         

                foreach (var keyword in keywords)
                {
                    var x = new NestedQuery() {
                        Path = "keywords",
                        Query = new BoolQuery() {
                            Must = new QueryContainer[]
                                {
                                    new MatchQuery { Field = "keywords.value", Query = keyword.Value},
                                    new MatchQuery { Field = "keywords.vocab", Query = keyword.Vocab}
                                }
                            }
                    };

                    keywordQueries.Add(x);
                }
            }

            if (commonQueries.Any() || keywordQueries.Any())
            {           
                fullQuery = new BoolQuery()
                {
                    Filter = new QueryContainer[]
                    {
                        new MatchQuery { Field = "site", Query = ES_SITE }
                    },
                    Should = commonQueries.ToArray(),
                    Must = keywordQueries.ToArray(),
                    MinimumShouldMatch = commonQueries.Any() ? 1 : 0
                };
            }
            else
            {
                // If we have no text search then make sure we are only matching on 
                // the correct site
                fullQuery = new MatchQuery { Field = "site", Query = ES_SITE };
            }

            return fullQuery;
        }
    }
}
