using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using dotenv.net;
using Datahub.Web.Search;
using Datahub.Web.Config;

namespace Datahub.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            if (System.IO.File.Exists(".env"))
            { 
                DotEnv.Config();
            }

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // inject env variables
            services.AddSingleton(typeof(IEnv), new Env());
            services.AddSingleton(typeof(IPublicEnv), new PublicEnv());

            // register dependencies
            services.AddTransient<IElasticsearchService, ElasticsearchService>();
            services.AddTransient<ISearchBuilder, SearchBuilder>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // disable HTTPS enforcement for now
                // app.UseHsts();
            }

            // disable HTTPS enforcement for now
            // app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
