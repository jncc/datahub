using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web.Models;
using Datahub.Web.Search;
using Nest;
using Datahub.Web;

public class QueryController : Controller
{
    readonly IEnv env;
    readonly ISearchBuilder searchBuilder;
    readonly IElasticsearchService esService;
    
    public QueryController(IEnv env, ISearchBuilder searchBuilder, IElasticsearchService esService)
    {
        this.env = env;
        this.searchBuilder = searchBuilder;
        this.esService = esService;
    }

    [HttpGet("/query")]
    public IActionResult Query(SearchParams input)
    {
        // var search = searchBuilder.BuildQuery(input);
        var client = esService.Client();

        var keywords = searchBuilder.ParseKeywords(input.k);
        var query = ElasticsearchService.BuildDatahubQuery(env.ES_SITE, input.q, keywords);

        // perhaps use SerializationFormatting.None?
        string json = client.RequestResponseSerializer.SerializeToString(query);
        return Ok(json);
    }
}
