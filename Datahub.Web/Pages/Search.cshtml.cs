using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Datahub.Web.Search;
using Datahub.Web.Config;
using Nest;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Web.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IEnv _env;
        private readonly ISearchBuilder _searchBuilder;
        private readonly IElasticsearchService _esService;

        // view model properties
        public SearchParams SearchParams { get; private set; }
        public List<Keyword> Keywords { get; private set; }
        public ISearchResponse<SearchResult> Results { get; private set; }

        public SearchModel(IEnv env, ISearchBuilder searchBuilder, IElasticsearchService esService)
        {
            this._env = env;
            this._searchBuilder = searchBuilder;
            this._esService = esService;
        }
        
        public async Task OnGetAsync(SearchParams input)
        {
            SearchParams = input;
            Keywords = _searchBuilder.ParseKeywords(input.k);

            var search = _searchBuilder.BuildQuery(input);
            var client = _esService.Client();
            Results = await client.SearchAsync<SearchResult>(search);
        }
    }
}
