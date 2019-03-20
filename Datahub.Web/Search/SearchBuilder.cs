
using System;
using System.Collections.Generic;
using System.Linq;
using Datahub.Web.Search;
using Datahub.Web.Models;
using Nest;

namespace Datahub.Web.Search
{
    public interface ISearchBuilder
    {
        List<Keyword> ParseKeywords(string[] keywords);
        SearchDescriptor<SearchResult> BuildQuery(SearchParams input);
    }

    public class SearchBuilder : ISearchBuilder
    {
        readonly IEnv _env;
        readonly IElasticsearchService _esService;

        public SearchBuilder(IEnv env, IElasticsearchService esService)
        {
            this._env = env;
            this._esService = esService;
        }

        public SearchDescriptor<SearchResult> BuildQuery(SearchParams input)
        {
            return new SearchDescriptor<SearchResult>()
                .Index(_env.ES_INDEX)
                .From(ElasticsearchService.GetStartFromPage(input.p, input.size)) // todo check this
                .Size(input.size)
                .Source(src => src
                    .IncludeAll()
                    .Excludes(e => e
                        .Field(f => f.Content)
                    )
                )
                .Query(l => ElasticsearchService.BuildDatahubQuery(input.q, ParseKeywords(input.k)))
                .Highlight(h => h
                    .Fields(f => f.Field(x => x.Content )
                                .Type(HighlighterType.Fvh)
                                .Order(HighlighterOrder.Score)
                                .NumberOfFragments(1))
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
    }
}
