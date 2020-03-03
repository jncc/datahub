using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Datahub.Web.Data;
using Datahub.Web.Models;
using Datahub.Web.Pages.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Datahub.Web.Pages
{
    public class AssetModel : PageModel
    {
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IDynamodbService _db;
        private readonly Env _env;

        public AssetModel(IHostingEnvironment hostingEnv, IDynamodbService db, Env env)
        {
            _hostingEnv = hostingEnv;
            _db = db;
            _env = env;
        }

        public Asset Asset { get; set; }

        public async Task<IActionResult> OnGetAsync(string assetId)
        {
            try {
                if (_env.DB_TABLE.IsNotBlank())
                {
                    this.Asset = await _db.GetAsset(assetId);
                }
                else
                {
                    var assets = await JsonLoader.LoadAssets(_hostingEnv.ContentRootPath);
                    this.Asset = assets.Single(a => a.Id == assetId);
                }
            } catch (NullReferenceException) {
                return NotFound();
            }

            return Page();
        }
    }
}
