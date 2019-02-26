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
using Datahub.Web.Config;
using Nest;
using Datahub.Web;

public class QueryController : Controller
{
    private readonly IEnv _env;
    private readonly ISearchBuilder _searchBuilder;
    private readonly IElasticsearchService _esService;
    
    public QueryController(IEnv env, ISearchBuilder searchBuilder, IElasticsearchService esService)
    {
        this._env = env;
        this._searchBuilder = searchBuilder;
        this._esService = esService;
    }

    [HttpGet("/query")]
    public IActionResult Query(SearchParams input)
    {
        // var search = searchBuilder.BuildQuery(input);
        var client = _esService.Client();

        var keywords = _searchBuilder.ParseKeywords(input.k);
        var query = ElasticsearchService.BuildDatahubQuery(_env.ES_SITE, input.q, keywords);

        // perhaps use SerializationFormatting.None?
        string json = client.RequestResponseSerializer.SerializeToString(query);
        return Ok(json);
    }
}
