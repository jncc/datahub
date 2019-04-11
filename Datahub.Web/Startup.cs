
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using dotenv.net;
using Datahub.Web.Search;
using Datahub.Web.Models;
using Datahub.Web.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace Datahub.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // initialise env vars for local development
            DotEnv.Config(false); // (do not throw if .env file doen't exist)
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // inject env variables
            services.AddSingleton(typeof(IEnv), new Env());

            // register dependencies
            services.AddTransient<ILayoutViewModel, LayoutViewModel>();
            services.AddTransient<IElasticsearchService, ElasticsearchService>();
            services.AddTransient<ISearchBuilder, SearchBuilder>();
            services.AddTransient<IDynamodbService, DynamodbService>();

            // services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IEnv envVars)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                if (Convert.ToBoolean(envVars.FORCE_HTTPS)){
                    var redirectOptions = new RewriteOptions().AddRedirectToProxiedHttps();
                    app.UseRewriter(redirectOptions);
                }
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }

    // Redirect to HTTPS doesn't work in conventional pattern via AWS ELB for some reason, need to 
    // scan for the header and redirect manually
    // https://stackoverflow.com/questions/46701670/net-core-https-with-aws-load-balancer-and-elastic-beanstalk-doesnt-work
    public static class RedirectToProxiedHttpsExtensions
    {
        public static RewriteOptions AddRedirectToProxiedHttps(this RewriteOptions options)
        {
            options.Rules.Add(new RedirectToProxiedHttpsRule());
            return options;
        }
    }

    public class RedirectToProxiedHttpsRule : IRule
    {
        public virtual void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;

            // #1) Did this request start off as HTTP?
            string reqProtocol;
            if (request.Headers.ContainsKey("X-Forwarded-Proto"))
            {
                reqProtocol = request.Headers["X-Forwarded-Proto"][0];
            }
            else
            {
                reqProtocol = (request.IsHttps ? "https" : "http");
            }


            // #2) If so, redirect to HTTPS equivalent
            if (reqProtocol != "https")
            {
                string url = UriHelper.BuildAbsolute(
                        "https",
                        request.Host,
                        request.PathBase,
                        request.Path,
                        request.QueryString).ToString();

                context.HttpContext.Response.Redirect(url, true);
            }
        }
    }
}
