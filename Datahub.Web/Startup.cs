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
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Amazon.S3;

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
            services.AddTransient<IS3Service, S3Service>();

            // services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMemoryCache();
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

                var redirectOptions = new RewriteOptions()
                    .AddRedirectToProxiedHttps();
                app.UseRewriter(redirectOptions);
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            // Use custom Sitemap / Robots TXT
            app.UseSitemapMiddleware();

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

    public static class CacheKeys
    {
        public static string RobotsTxt { get { return "_RobotsTxt"; } }
        public static string Sitemap { get { return "_Sitemap"; } }
    }

    public class SitemapMiddleware
    {
        private static readonly PathString _robotsTxtPath = new PathString("/robots.txt");
        private static readonly PathString _sitemapPath = new PathString("/sitemap.xml");

        private IEnv _env;
        private IMemoryCache _cache;
        private IS3Service _s3Service;
        private RequestDelegate _next;

        public SitemapMiddleware(RequestDelegate next, IMemoryCache cache, IEnv env, IS3Service s3Service)
        {
            _cache = cache;
            _env = env;
            _s3Service = s3Service;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            TimeSpan cacheSpan = TimeSpan.FromHours(6);
            if (context.Request.Path == _sitemapPath)
            {              
                if (!_cache.TryGetValue(CacheKeys.Sitemap, out byte[] SitemapBytes))
                {
                    MemoryCacheEntryOptions cacheEntryOptions;

                    try
                    {
                        MemoryStream mStream = new MemoryStream();
                        Stream sitemapStream = await _s3Service.GetObjectAsStream(_env.SITEMAP_S3_BUCKET, _env.SITEMAP_S3_KEY);
                        await sitemapStream.CopyToAsync(mStream);

                        SitemapBytes = mStream.ToArray();
                        cacheSpan = TimeSpan.FromHours(6);
                        cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(cacheSpan);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FileNotFoundException || ex is AmazonS3Exception)
                        {
                            // TODO: Log failure
                            SitemapBytes = Encoding.UTF8.GetBytes(string.Format("<sitemap><url><loc>{0}</loc></url></sitemap>", _env.SELF_REFERENCE_URL));
                            cacheSpan = TimeSpan.FromMinutes(30);
                            cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(cacheSpan);
                        }

                        throw ex;
                    }

                    _cache.Set(CacheKeys.Sitemap, SitemapBytes, cacheEntryOptions);
                }

                Stream stream = context.Response.Body;
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/xml";
                context.Response.Headers.Add("Cache-Control", $"max-age={cacheSpan}");

                await stream.WriteAsync(SitemapBytes);
            }
            else if (context.Request.Path == _robotsTxtPath)
            {
                if (!_cache.TryGetValue(CacheKeys.RobotsTxt, out byte[] RobotBytes))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("User-agent: *");
                    sb.AppendLine("Dissallow: /css/");
                    sb.AppendLine("Dissallow: /images/");
                    sb.AppendLine("Dissallow: /js/");
                    sb.AppendLine("Dissallow: /lib/");
                    sb.AppendLine(string.Format(format: "Sitemap: {0}",
                        UriHelper.BuildAbsolute(
                            context.Request.IsHttps ? "https" : "http",
                            new HostString(_env.SELF_REFERENCE_URL),
                            _sitemapPath
                        ).ToString()
                    ));

                    RobotBytes = Encoding.UTF8.GetBytes(sb.ToString());

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    _cache.Set(CacheKeys.RobotsTxt, RobotBytes, cacheEntryOptions);
                }

                Stream stream = context.Response.Body;
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                context.Response.Headers.Add("Cache-Control", $"max-age={TimeSpan.FromDays(1)}");
                
                await stream.WriteAsync(RobotBytes);
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSitemapMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SitemapMiddleware>();
        }
    }
}
