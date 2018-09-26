

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
                assets = new List<Asset>();

                var topcatPath = Path.Combine(appRootPath, "Data", "topcat");
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "crick_framework.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "doi_rock.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "making_eo_work_phase_1.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "making_eo_work_phase_2.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "npms_2015.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "uk_species_conservation_designation_master_list.json"))));

                var manualPath = Path.Combine(appRootPath, "Data", "manual");
                var csmPath = Path.Combine(manualPath, "common-standards-monitoring");
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_freshwater.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_grasslands.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_heathlands.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_wetland.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_marine.json"))));

                var nvcPath = Path.Combine(manualPath, "national-vegetation-classification");
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_users_handbook.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_communities_and_sub_communities.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_mires_heaths.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_upland_vegetation.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_woodland.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_summary_grassland_montane.json"))));
                assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_summary_woodland.json"))));
            }

            return assets;
        } 
    }
}