using System;
using System.Net.Http;
using System.Threading.Tasks;
using Aws4RequestSigner;

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

        }



        async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
        {
            var signer = new AWS4RequestSigner(env.AWS_ACCESSKEY, env.AWS_SECRETACCESSKEY);
            return await signer.Sign(request, "es", env.AWS_REGION);
        }        
    }
}
