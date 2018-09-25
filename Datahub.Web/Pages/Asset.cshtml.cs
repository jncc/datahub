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
        readonly IHostingEnvironment env;
        public AssetModel(IHostingEnvironment env)
        {
            this.env = env;
        }

        public Asset Asset { get; set; }


        public async Task OnGetAsync(string assetId)
        {
            var assets = await JsonLoader.LoadAssets(this.env.ContentRootPath);
            this.Asset = assets.Single(a => a.Id == assetId);
        }
    }
}
