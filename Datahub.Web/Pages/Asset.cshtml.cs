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
    public class AssetModel : PageModel
    {
        private readonly IHostingEnvironment _env;
        public AssetModel(IHostingEnvironment env)
        {
            _env = env;
        }

        public Asset Asset { get; set; }


        public async Task OnGetAsync(string assetId)
        {
            var assets = await JsonLoader.LoadAssets(_env.ContentRootPath);
            this.Asset = assets.Single(a => a.Id == assetId);
        }
    }
}
