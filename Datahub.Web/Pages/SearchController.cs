using System;
using Microsoft.AspNetCore.Mvc;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Nest;
using Datahub.Web.Models;
using Datahub.Web.Elasticsearch;

namespace Datahub.Web.Pages
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ElasticClient _client;

        public SearchController(IElasticsearchService elasticsearchService)
        {
            _client = elasticsearchService.Client();
        }

        public ISearchResponse<SearchResult> Get(string index = "_all", int startIndex = 0, int size = 10, string site = "datahub")
        {
            var results = _client.Search<SearchResult>(s => s
                .Index(index)
                .From(startIndex)
                .Size(size)
                .Source(src => src
                    .IncludeAll()
                    .Excludes(e => e
                        .Field(f => f.Content)
                    )
                )
                .Query(q => 
                    q.Match(m => m
                        .Field(f => f.Site)
                        .Query(site)
                    ) 
                    &&
                    q.CommonTerms(c => c
                        .Field(f => f.Content)
                        .Query("sea")
                        .CutoffFrequency(0.001)
                        .LowFrequencyOperator(Operator.And)
                    )
                )
                .Highlight(h => h
                    .Fields(f => f.Field(x => x.Content))
                )
                .RequestConfiguration(rc => rc.DisableDirectStreaming())
            );

            return results;
        }

    }
}