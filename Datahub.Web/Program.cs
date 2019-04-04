using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Web
{
    public class Program
    {
        public static void Main(string[] args) { 
            IHostingEnvironment env = null;

            CreateWebHostBuilder(args)
            .ConfigureServices(services =>
            {
                env = services
                    .Where(x => x.ServiceType == typeof(IHostingEnvironment))
                    .Select(x => (IHostingEnvironment)x.ImplementationInstance)
                    .First();
            })
            .UseKestrel(options => {
                if(!env.IsDevelopment()){
                    options.ListenAnyIP(80);
                    options.ListenAnyIP(443);    
                }
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .Build()
            .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
