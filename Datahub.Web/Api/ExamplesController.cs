using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web;
using Datahub.Web.Models;
using Datahub.Web.Search;
using Nest;

public class ExamplesController : Controller
{
    readonly Env env;

    public ExamplesController(Env env)
    {
        this.env = env;
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
        var signer = new AWS4RequestSigner(env.AWS_ACCESS_KEY_ID, env.AWS_SECRET_ACCESS_KEY);
        return await signer.Sign(request, "es", env.AWS_DEFAULT_REGION);
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
        var uri = new UriBuilder
        {
            Host = env.ES_ENDPOINT_HOST,
            Scheme = env.ES_ENDPOINT_SCHEME,
            Port = Convert.ToInt32(env.ES_ENDPOINT_PORT),
        };

        return uri.ToString();
    }
}
