using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Datahub.Web.Elasticsearch;
using Nest;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Web.Pages
{
    public class SearchModel : PageModel
    {
        const string ES_SITE = "datahub";
        
        const int DefaultPage = 0;
        const int DefaultPageSize = 10;

        readonly IEnv _env;
        readonly IElasticsearchService _esService;

        public ISearchResponse<SearchResult> Results { get; set; }

        public string QueryString      { get; private set; }
        public int    CurrentPage      { get; private set; }
        public int    CurrentPageSize  { get; private set; }
        public string ESQueryJson      { get; private set; }

        public List<Keyword> Keywords { get; set; }

        public SearchModel(IEnv env, IElasticsearchService elasticsearchService)
        {
            _env = env;
            _esService = elasticsearchService;
        }
        
        public async Task OnGetAsync(SearchParams input)
        {
            System.Console.WriteLine("Hello");
            System.Console.WriteLine(input.q);

            CurrentPageSize = input.size; //todo rename 'size'
            CurrentPage = input.p;
            Keywords = ParseKeywords(input.k);

            var search = BuildQuery(input);

            var client = _esService.Client();

            var json = client.RequestResponseSerializer.SerializeToString(search);
            System.Console.WriteLine(json);

            Results = await client.SearchAsync<SearchResult>(search);
        }

        SearchDescriptor<SearchResult> BuildQuery(SearchParams input)
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
                .Query(l => ElasticsearchService.BuildDatahubQuery(input.q, ParseKeywords(input.k), ES_SITE))
                .Highlight(h => h
                    .Fields(f => f.Field(x => x.Content))
                    .PreTags("<b>")
                    .PostTags("</b>")
                );
        }

        List<Keyword> ParseKeywords(string[] keywords)
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

        IEnumerable<Asset> ApplyQuery(IEnumerable<Asset> assets)
        {
            return assets.Where(a => a.Metadata.Keywords.Any(k =>
                Keywords.Any(kx => kx.Vocab == k.Vocab && kx.Value == k.Value)
                ));
        }
    }
}
