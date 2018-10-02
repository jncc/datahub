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
    public class SearchModel : PageModel
    {
        readonly IHostingEnvironment env;
        public SearchModel(IHostingEnvironment env)
        {
            this.env = env;
        }

        public List<SearchResultModel> Results { get; set; }
        public List<KeywordModel> Keywords { get; set; }


        public async Task OnGetAsync(string q, string[] k)
        {
            // populate Keywords to show which are filtered on
            this.Keywords = ParseKeywords(k);

            // populate Results
            var assets = await JsonLoader.LoadAssets(this.env.ContentRootPath);
            this.Results = assets.Select(a => new SearchResultModel {
              Id = a.Id,
              Title = a.Metadata.Title,
              Abstract = a.Metadata.Abstract.Substring(0, 300) + " ...",
              DatasetReferenceDate = a.Metadata.DatasetReferenceDate,
              ResourceType = a.Metadata.ResourceType,
            })
            .Take(4)
            .ToList();
        }

        List<KeywordModel> ParseKeywords(string[] keywords)
        {
            return keywords.Select(k =>
            {
                int lastIndexOfSlash = k.LastIndexOf('/');
                if (lastIndexOfSlash > 0)
                {
                    // has a slash, so assume this keyword this has a vocab
                    string vocab = k.Substring(0, lastIndexOfSlash);
                    string value = k.Substring(lastIndexOfSlash + 1);
                    return new KeywordModel { Vocab = vocab, Value = value };
                }
                else
                {
                    return new KeywordModel { Vocab = null, Value = k };
                }
            }).ToList();
        }
    }
}
