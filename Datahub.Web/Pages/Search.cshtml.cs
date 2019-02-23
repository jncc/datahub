using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Datahub.Web.Search;
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
        readonly ISearchBuilder _searchBuilder;
        readonly IElasticsearchService _esService;

        public ISearchResponse<SearchResult> Results { get; set; }

        public string QueryString      { get; private set; }
        public int    CurrentPage      { get; private set; }
        public int    CurrentPageSize  { get; private set; }
        public string ESQueryJson      { get; private set; }

        public List<Keyword> Keywords { get; set; }

        public SearchModel(IEnv env, ISearchBuilder searchBuilder, IElasticsearchService esService)
        {
            _env = env;
            _searchBuilder = searchBuilder;
            _esService = esService;
        }
        
        public async Task OnGetAsync(SearchParams input)
        {
            System.Console.WriteLine("Hello");
            System.Console.WriteLine(input.q);

            CurrentPageSize = input.size; //todo rename 'size'
            CurrentPage = input.p;
            Keywords = _searchBuilder.ParseKeywords(input.k);

            var search = _searchBuilder.BuildQuery(input);

            var client = _esService.Client();

            var json = client.RequestResponseSerializer.SerializeToString(search);
            System.Console.WriteLine(json);

            Results = await client.SearchAsync<SearchResult>(search);
        }
    }
}
