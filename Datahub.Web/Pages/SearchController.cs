﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        private const string Index = "main";
        private const string Site = "datahub";
        private const int Start = 0;
        private const int Size = 10;

        public SearchController(IElasticsearchService elasticsearchService)
        {
            _client = elasticsearchService.Client();
        }

        public IReadOnlyCollection<IHit<SearchResult>> Get(string q, int startIndex = Start, int size = Size)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                return _client.Search<SearchResult>(s => s
                    .Index(Index)
                    .From(startIndex)
                    .Size(size)
                    .Source(src => src
                        .IncludeAll()
                        .Excludes(e => e
                            .Field(f => f.Content)
                        )
                    )
                    .Query(l =>
                        l.Match(m => m
                            .Field(f => f.Site)
                            .Query(Site)
                        )
                        &&
                        l.CommonTerms(c => c
                            .Field(f => f.Content)
                            .Query(q)
                            .CutoffFrequency(0.001)
                            .LowFrequencyOperator(Operator.Or)
                        )
                    )
                    .Highlight(h => h
                        .Fields(f => f.Field(x => x.Content))
                        .PreTags("<b>")
                        .PostTags("</b>")
                    )
                ).Hits;
            }
            return null;
        }

    }
}
