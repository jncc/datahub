

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Datahub.Web.Models;
using Newtonsoft.Json;

namespace Datahub.Web.Data
{
    public static class JsonLoader
    {
        // keep a cache to save reloading on every request
        private static List<Asset> s_assets;
        public static async Task<List<Asset>> LoadAssets(string appRootPath)
        {
            if (s_assets == null)
            {
                s_assets = new List<Asset>();

                var topcatPath = Path.Combine(appRootPath, "Data", "topcat");
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "crick_framework.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "doi_rock.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "making_eo_work_phase_1.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "making_eo_work_phase_2.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "npms_2015.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(topcatPath, "uk_species_conservation_designation_master_list.json"))));

                var manualPath = Path.Combine(appRootPath, "Data", "manual");
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(manualPath, "ramsar.json"))));

                var csmPath = Path.Combine(manualPath, "common-standards-monitoring");
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_freshwater.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_grasslands.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_heathlands.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_lowland_wetland.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_marine.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(csmPath, "csm_coastal.json"))));

                var nvcPath = Path.Combine(manualPath, "national-vegetation-classification");
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_users_handbook.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_communities_and_sub_communities.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_mires_heaths.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_upland_vegetation.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_guide_woodland.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_summary_grassland_montane.json"))));
                s_assets.Add(JsonConvert.DeserializeObject<Asset>(await File.ReadAllTextAsync(Path.Combine(nvcPath, "nvc_summary_woodland.json"))));
            }

            return s_assets;
        }
    }
}
