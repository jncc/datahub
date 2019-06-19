using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web;
using Datahub.Web.Data;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

public class RobotsController : Controller
{
    private static readonly string _robotsTxtMemCacheKey = "_RobotsTxt";
    private readonly Env _env;
    private IMemoryCache _cache;

    public RobotsController(Env env, IMemoryCache cache)
    {
        _env = env;
        _cache = cache;
    }

    [HttpGet("/robots.txt")]
    public IActionResult GetRobotsTxt()
    {
        // If the robots.txt file is not in the MemoryCache create it and store that in the cache,
        // does not expire as this is a per instance setup and would require a re-deploy to modify
        // anyway
        if (!_cache.TryGetValue(_robotsTxtMemCacheKey, out byte[] RobotBytes))
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Dissallow: /css/");
            sb.AppendLine("Dissallow: /images/");
            sb.AppendLine("Dissallow: /js/");
            sb.AppendLine("Dissallow: /lib/");
            sb.AppendLine(string.Format(format: "Sitemap: {0}",
                UriHelper.BuildAbsolute(
                    this.Request.Scheme,
                    this.Request.Host,
                    "/sitemap.xml"
                ).ToString()
            ));

            RobotBytes = Encoding.UTF8.GetBytes(sb.ToString());

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);

            _cache.Set(_robotsTxtMemCacheKey, RobotBytes, cacheEntryOptions);
        }

        Response.Headers.Add("Cache-Control", $"max-age={TimeSpan.FromDays(1)}");
        return new FileContentResult(RobotBytes, "text/plain");
    }
}