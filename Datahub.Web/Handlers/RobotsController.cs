using System.Text;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web;
using Microsoft.Extensions.Caching.Memory;

public class RobotsController : Controller
{
    private readonly Env _env;

    public RobotsController(Env env, IMemoryCache cache)
    {
        _env = env;
    }

    [HttpGet("/robots.txt")]
    public IActionResult GetRobotsTxt()
    {
        var s = new StringBuilder();

        s.AppendLine("User-agent: *");
        s.AppendLine("Dissallow: /css/");
        s.AppendLine("Dissallow: /images/");
        s.AppendLine("Dissallow: /js/");
        s.AppendLine("Dissallow: /lib/");
        s.AppendLine($"Sitemap: {_env.BASE_URL}/sitemap.xml");
        s.AppendLine();

        return Ok(s.ToString());
    }
}
