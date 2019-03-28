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
    readonly ISearchBuilder searchBuilder;
    readonly IElasticsearchService esService;
    
    public QueryController(ISearchBuilder searchBuilder, IElasticsearchService esService)
    {
        this.searchBuilder = searchBuilder;
        this.esService = esService;
    }

    [HttpGet("/query")]
    public IActionResult Query(SearchParams input)
    {
        // var search = searchBuilder.BuildQuery(input);
        var client = esService.Client();

        var keywords = searchBuilder.ParseKeywords(input.k);
        var query = searchBuilder.BuildDatahubQuery(input.q, keywords);

        // perhaps use SerializationFormatting.None?
        string json = client.RequestResponseSerializer.SerializeToString(query);
        return Ok(json);
    }
}
