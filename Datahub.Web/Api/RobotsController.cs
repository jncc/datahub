using System;
using Microsoft.AspNetCore.Mvc;

public class RobotsController : Controller
{
    [HttpGet("/robots.txt")]
    public IActionResult RobotsTxt()
    {
        string content =
@"
User-agent: *
Allow: /

Sitemap: http://hub.jncc.gov.uk/sitemap.xml
";

        return Ok(content);
    }
}
