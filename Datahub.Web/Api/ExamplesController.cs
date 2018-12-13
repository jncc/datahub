using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class ExamplesController : Controller
{ 
    [HttpGet("/api/examples/esget")]
    public async Task<IActionResult> ESGet(int id)
    {
        string query = @"
            {
                ""query"" : {
                    ""simple_query_string"": { ""query"": ""sea"" }
                }
            }
            ".Trim();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(GetESEndpointUrl() + "main/_search"),
            Content = new StringContent(query, Encoding.UTF8, "application/json")
        };

        var signedRequest = await GetSignedRequest(request);
        var response = await new HttpClient().SendAsync(signedRequest);
        var responseString = await response.Content.ReadAsStringAsync();        

        return Ok(responseString);
    }

    async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
    {
        var signer = new AWS4RequestSigner(Env.Var.ESAwsAccessKey, Env.Var.ESAwsSecretAccessKey);
        return await signer.Sign(request, "ess", Env.Var.ESAwsRegion);
    }

    string GetESEndpointUrl()
    {
        // the reason for building the endpoint URL from host, scheme and port here is obscure
        // and apparently helps to solve a problem when reading environment variables on elastic beanstalk
        // (there's no need to make it this complicated outside elasticbeanstalk!)
        var uri = new UriBuilder() { Host = Env.Var.ESEndpointHost };
        if (!String.IsNullOrEmpty(Env.Var.ESEndpointScheme)) 
            uri.Scheme = Env.Var.ESEndpointScheme;
        if (!String.IsNullOrEmpty(Env.Var.ESEndpointPort))
            uri.Port = Int32.Parse(Env.Var.ESEndpointPort);

        return uri.ToString();
    }
}

/// <summary>
/// Provides environment variables.
/// </summary>
public class Env
{    
    static Env() { } // this is a singleton
    private static readonly Env env = new Env();

    public string ESAwsRegion          { get; private set; }
    public string ESAwsAccessKey       { get; private set; }
    public string ESAwsSecretAccessKey { get; private set; }
    public string ESEndpointHost       { get; private set; }
    public string ESEndpointScheme     { get; private set; }
    public string ESEndpointPort       { get; private set; }
    
    private Env()
    {
        ESAwsRegion = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_REGION");
        ESAwsAccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_ACCESSKEY");
        ESAwsSecretAccessKey = Environment.GetEnvironmentVariable("ELASTICSEARCH_AWS_SECRETACCESSKEY");
        ESEndpointHost = Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST");
        ESEndpointScheme = Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_SCHEME");
        ESEndpointPort = Environment.GetEnvironmentVariable("ELASTICSEARCH_HOST_PORT");
    }

    public static Env Var
    {
        get { return env; }
    }
}
