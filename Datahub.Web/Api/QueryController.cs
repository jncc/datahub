using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web.Models;
using Datahub.Web.Search;
using Nest;

public class QueryController : Controller
{
    [HttpGet("/query")]
    public IActionResult Query()
    {
        return Ok("Hello!");
    }
}
