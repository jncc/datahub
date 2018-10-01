using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Data;
using Datahub.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Datahub.Web.Pages
{
    public class SearchModel : PageModel
    {
        readonly IHostingEnvironment env;
        public SearchModel(IHostingEnvironment env)
        {
            this.env = env;
        }

        public List<SearchResult> Results { get; set; }


        public async Task OnGetAsync(string assetId)
        {
            var assets = await JsonLoader.LoadAssets(this.env.ContentRootPath);
            this.Results = assets.Select(a => new SearchResult {
              Title = a.Metadata.Title,
              Abstract = a.Metadata.Abstract.Substring(0, 300) + " ...",
              DatasetReferenceDate = a.Metadata.DatasetReferenceDate,
              ResourceType = a.Metadata.ResourceType,
            })
            .Take(4)
            .ToList();
        }
    }
}
