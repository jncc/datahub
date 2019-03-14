using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Web.Data;
using Datahub.Web.Models;
using Datahub.Web.Pages.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Datahub.Web.Pages
{
    public class AssetModel : PageModel
    {
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IDynamodbService _db;
        private readonly IEnv _env;

        public AssetModel(IHostingEnvironment hostingEnv, IDynamodbService db, IEnv env)
        {
            _hostingEnv = hostingEnv;
            _db = db;
            _env = env;
        }

        public Asset Asset { get; set; }

        public async Task OnGetAsync(string assetId)
        {
            if (_env.DB_TABLE.IsNotBlank())
            {
                this.Asset = await _db.GetAsset(assetId);
            }
            else
            {
                var assets = await JsonLoader.LoadAssets(_hostingEnv.ContentRootPath);
                this.Asset = assets.Single(a => a.Id == assetId);
            }
        }
    }
}
