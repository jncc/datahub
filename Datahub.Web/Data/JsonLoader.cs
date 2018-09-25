

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Newtonsoft.Json;

namespace Datahub.Web.Data {
    public static class JsonLoader {
        // keep a cache to save reloading on every request
        static List<Asset> assets;
        public static async Task<List<Asset>> LoadAssets(string appRootPath) {

            if (assets == null) {
                //var path = Path.Combine(appRootPath, "Data", "assets.json");
                //string json = await File.ReadAllTextAsync(path);
                //assets = JsonConvert.DeserializeObject<List<Asset>>(json);

                assets = new List<Asset>();

                var path = Path.Combine(appRootPath, "Data", "datasets", "rock-uk-shelf-seabed", "doi_rock.json");
                var json = await File.ReadAllTextAsync(path);
                assets.Add(JsonConvert.DeserializeObject<Asset>(json));

                var publicationPath = Path.Combine(appRootPath, "Data", "publications");
                json = await File.ReadAllTextAsync(Path.Combine(publicationPath, "common-standards-monitoring", "csm_freshwater.json"));
                assets.Add(JsonConvert.DeserializeObject<Asset>(json));
            }

            return assets;
        } 
    }
}