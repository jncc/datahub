using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Newtonsoft.Json;

namespace example_uploader
{
    class Program
    {
        Env env = new Env();

        static void Main()
        {
            Console.WriteLine("Hello World!");
            new Program().Example().GetAwaiter().GetResult();
        }

        async Task Example()
        {
            Console.WriteLine($"Using AWS Access Key {env.AWS_ACCESSKEY}");

            var doc = new { id = "1000000" };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(new Uri(env.HUB_API_ENDPOINT), "assets"),
                Content = new StringContent(
                    JsonConvert.SerializeObject(doc),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var signedRequest = await GetSignedRequest(request);
            var response = await new HttpClient().SendAsync(signedRequest);
            var responseString = await response.Content.ReadAsStringAsync();        

            Console.WriteLine(responseString);
        }



        async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
        {
            var signer = new AWS4RequestSigner(env.AWS_ACCESSKEY, env.AWS_SECRETACCESSKEY);
            return await signer.Sign(request, "es", env.AWS_REGION);
        }        
    }
}
