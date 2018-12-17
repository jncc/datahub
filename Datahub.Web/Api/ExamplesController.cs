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
using Datahub.Web.Elasticsearch;
using Nest;

public class ExamplesController : Controller
{
    private readonly ElasticClient _client;
    private readonly IElasticsearchService _elasticsearchService;

    public ExamplesController(IElasticsearchService elasticsearchService)
    {
        _client = elasticsearchService.Client();
        _elasticsearchService = elasticsearchService;
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
        var signer = new AWS4RequestSigner(Env.Var.ESAwsAccessKey, Env.Var.ESAwsSecretAccessKey);
        return await signer.Sign(request, "es", Env.Var.ESAwsRegion);
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
                        {{ ""term"": {{ ""site"": ""{2}"" }} }}
                    ],
                    ""must"": [
                        {{ ""common"": {{ ""content"": {{ ""query"": ""{3}"", ""cutoff_frequency"": 0.001, ""low_freq_operator"": ""or"" }} }} }}
                    ]
                }}
            }},
            ""highlight"": {{
                ""fields"": {{ ""content"": {{}} }}
            }}
        }}", start, size, site, query).Trim();

        UriBuilder uriBuilder = new UriBuilder(GetESEndpointUrl());
        uriBuilder.Path = "main/_search";


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
        var uri = new UriBuilder() { Host = Env.Var.ESEndpointHost };
        if (!string.IsNullOrEmpty(Env.Var.ESEndpointScheme)) 
            uri.Scheme = Env.Var.ESEndpointScheme;
        if (!string.IsNullOrEmpty(Env.Var.ESEndpointPort))
            uri.Port = int.Parse(Env.Var.ESEndpointPort);

        return uri.ToString();
    }
}

/// <summary>
/// Provides environment variables.
/// </summary>
public class Env
{    
    static Env() { } // singleton to avoid reading a variable more than once
    private static readonly Env env = new Env();

    public string ESAwsRegion          { get; private set; }
    public string ESAwsAccessKey       { get; private set; }
    public string ESAwsSecretAccessKey { get; private set; }
    public string ESEndpointHost       { get; private set; }
    public string ESEndpointScheme     { get; private set; }
    public string ESEndpointPort       { get; private set; }
    
    private Env()
    {
        ESAwsRegion = GetVariable("ELASTICSEARCH_AWS_REGION");
        ESAwsAccessKey = GetVariable("ELASTICSEARCH_AWS_ACCESSKEY");
        ESAwsSecretAccessKey = GetVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY");
        ESEndpointHost = GetVariable("ELASTICSEARCH_HOST");
        ESEndpointScheme = GetVariable("ELASTICSEARCH_HOST_SCHEME");
        ESEndpointPort = GetVariable("ELASTICSEARCH_HOST_PORT", true);
    }

    string GetVariable(string variable, bool optional = false)
    {
        if (optional)
        {
            return null;
        }
        return Environment.GetEnvironmentVariable(variable)
            ?? throw new Exception($"The environment variable {variable} couldn't be read. You may need to define it in your .env file.");
    }

    public static Env Var
    {
        get { return env; }
    }
}
