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

public class SitemapController : Controller
{
    private static readonly string _robotsTxtMemCacheKey = "_RobotsTxt";
    private static readonly string _sitemapMemCacheKey = "_Sitemap";
    private static readonly string _sitemapPath = "/sitemap.xml";
    private readonly Env _env;
    private IMemoryCache _cache;
    private IS3Service _s3Service;

    public SitemapController(Env env, IMemoryCache cache, IS3Service s3Service)
    {
        _env = env;
        _cache = cache;
        _s3Service = s3Service;
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> GetSitemap()
    {
        // Default time span of 6 hours to cache the sitemap
        TimeSpan cacheSpan = TimeSpan.FromHours(6);

        // Try and retrieve the Sitemap bytes from the MemoryCache, if its not present, fetch it from
        // S3 and cache the result
        if (!_cache.TryGetValue(_sitemapMemCacheKey, out byte[] sitemapBytes))
        {
            // TODO: Try to retrieve from S3, if that fails, log the error and if it is a AmazonS3
            // Exception return a default sitemap.xml then set the cache to expire in 30 minutes to try again
            // otherwise return the byte array representation of the sitemap.xml?
            var mStream = new MemoryStream();
            var sitemapStream = await _s3Service.GetObjectAsStream(_env.SITEMAP_S3_BUCKET, _env.SITEMAP_S3_KEY);
            await sitemapStream.CopyToAsync(mStream);

            sitemapBytes = mStream.ToArray();
            cacheSpan = TimeSpan.FromHours(6);
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(cacheSpan);

            _cache.Set(_sitemapMemCacheKey, sitemapBytes, cacheEntryOptions);
        }

        Response.Headers.Add("Cache-Control", $"max-age={cacheSpan}");
        return new FileContentResult(sitemapBytes, "application/xml");
    }
}
