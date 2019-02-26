using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web.Models;
using Datahub.Web.Search;
using Datahub.Web.Config;
using Nest;

public class ExamplesController : Controller
{
    private readonly IEnv _env;

    public ExamplesController(IEnv env)
    {
        this._env = env;
    }

    [HttpGet("/api/examples/esget")]
    public async Task<IActionResult> ESGet(string q = null, List<Keyword> k = null, string site = "datahub", int start = 0, int size = 10)
    {
        var signedRequest = await GetSignedRequest(BuildSearchQuery(q, k, site, start, size));
        var response = await new HttpClient().SendAsync(signedRequest);
        var responseString = await response.Content.ReadAsStringAsync();        

        return Ok(responseString);
    }

    async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
    {
        var signer = new AWS4RequestSigner(_env.ES_AWS_ACCESSKEY, _env.ES_AWS_SECRETACCESSKEY);
        return await signer.Sign(request, "es", _env.ES_AWS_REGION);
    }


    HttpRequestMessage BuildSearchQuery(string query = null, List<Keyword> keywords = null, string site = null, int start = 0, int size = 10)
    {
        string q = string.Format(@"{{
            ""_source"": {{ ""excludes"": [ ""content"" ] }},
            ""from"": {0},
            ""size"": {1},
            ""query"": {{
                ""bool"": {{
                    ""filter"": [
                        {{ ""match"": {{ ""site"": ""{2}"" }} }}
                    ],
                    ""must"": [
                        {{ ""common"": {{ ""content"": {{ ""query"": ""{3}"", ""cutoff_frequency"": 0.001, ""low_freq_operator"": ""or"" }} }} }}
                    ],
                    ""should"": [
                        {{ ""common"": {{ ""title"": {{ ""query"": ""{3}"", ""cutoff_frequency"": 0.001, ""low_freq_operator"": ""or"" }} }} }}
                    ]
                }}
            }},
            ""highlight"": {{
                ""fields"": {{ ""content"": {{}} }}
            }}
        }}", start, size, site, query).Trim();

        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(GetESEndpointUrl() + "main/_search"),
            Content = new StringContent(q, Encoding.UTF8, "application/json")
        };
        
    }

    string GetESEndpointUrl()
    {
        // the reason for building the endpoint URL from host, scheme and port here is obscure
        // and apparently helps to solve a problem when reading environment variables on elastic beanstalk
        // (there's no need to make it this complicated outside elasticbeanstalk!)
        var uri = new UriBuilder() { Host = _env.ES_ENDPOINT_HOST };
        if (!string.IsNullOrEmpty(_env.ES_ENDPOINT_SCHEME)) 
            uri.Scheme = _env.ES_ENDPOINT_SCHEME;
        if (!string.IsNullOrEmpty(_env.ES_ENDPOINT_PORT))
            uri.Port = int.Parse(_env.ES_ENDPOINT_PORT);

        return uri.ToString();
    }
}
