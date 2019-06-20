using System;
using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [HttpGet("/error")]
    public IActionResult Error()
    {
        throw new Exception("Well, that escalated quickly.");
    }
}
