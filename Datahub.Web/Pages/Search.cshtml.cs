using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Datahub.Web.Elasticsearch;
using Nest;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Web.Pages
{
    public class SearchModel : PageModel
    {
        const string ES_INDEX = "test";
        const string ES_SITE = "datahub";
        
        const int DefaultSize = 10;
        const int DefaultStart = 0;

        private readonly IHostingEnvironment _env;
        private readonly IElasticsearchService _esService;

        public ISearchResponse<SearchResult> Results { get; set; }

        public string QueryString      { get; private set; }
        public int    CurrentPage      { get; private set; }
        public int    CurrentPageSize  { get; private set; }

        public List<Keyword> Keywords { get; set; }

        public SearchModel(IHostingEnvironment env, IElasticsearchService elasticsearchService)
        {
            _env = env;
            _esService = elasticsearchService;
        }
        
        public async Task OnGetAsync(string q, string[] k, int p = 1, int size = DefaultSize)
        {
            CurrentPageSize = size;
            CurrentPage = p;

            Keywords = ParseKeywords(k);

            Results = await _esService.Client().SearchAsync<SearchResult>(s => s
                .Index(ES_INDEX)
                .From(ElasticsearchService.GetStartFromPage(p, size)) // todo check this
                .Size(size)
                .Source(src => src
                    .IncludeAll()
                    .Excludes(e => e
                        .Field(f => f.Content)
                    )
                )
                .Query(l => ElasticsearchService.BuildDatahubQuery(q, ParseKeywords(k), ES_SITE))
                .Highlight(h => h
                    .Fields(f => f.Field(x => x.Content))
                    .PreTags("<b>")
                    .PostTags("</b>")
                )
            );
        }

        private List<Keyword> ParseKeywords(string[] keywords)
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

        private IEnumerable<Asset> ApplyQuery(IEnumerable<Asset> assets)
        {
            return assets.Where(a => a.Metadata.Keywords.Any(k =>
                Keywords.Any(kx => kx.Vocab == k.Vocab && kx.Value == k.Value)
                ));
        }
    }
}
