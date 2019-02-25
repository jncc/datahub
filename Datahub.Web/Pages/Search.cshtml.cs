using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Datahub.Web.Search;
using Nest;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Web.Pages
{
    public class SearchModel : PageModel
    {
        readonly ISearchBuilder searchBuilder;
        readonly IElasticsearchService esService;

        // view model properties
        public SearchParams SearchParams { get; private set; }
        public List<Keyword> Keywords { get; private set; }
        public ISearchResponse<SearchResult> Results { get; private set; }

        public SearchModel(ISearchBuilder searchBuilder, IElasticsearchService esService)
        {
            this.searchBuilder = searchBuilder;
            this.esService = esService;
        }
        
        public async Task OnGetAsync(SearchParams input)
        {
            SearchParams = input;
            Keywords = searchBuilder.ParseKeywords(input.k);

            var search = searchBuilder.BuildQuery(input);
            var client = esService.Client();
            Results = await client.SearchAsync<SearchResult>(search);
        }
    }
}
