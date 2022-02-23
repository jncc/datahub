
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
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http.Extensions;
using Datahub.Web.Pages.Helpers;
using Westwind.AspNetCore.Markdown;

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

            var env = new Env();
            services.AddSingleton(env);
            services.AddTransient<LayoutViewModel>();
            
            var dynamodbServiceType = env.DB_TABLE.IsBlank() ? typeof(LocalDevDynamodbService) : typeof(DynamodbService);
            services.AddTransient(typeof(IDynamodbService), dynamodbServiceType);

            services.AddTransient<IElasticsearchService, ElasticsearchService>();
            services.AddTransient<ISearchBuilder, SearchBuilder>(); 
            services.AddTransient<IS3Service, S3Service>();

            services.AddMarkdown();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Env envVars)
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

            // Add X-Frame-Options header to prevent clickjacking
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc();
            
            app.UseMarkdown();
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
