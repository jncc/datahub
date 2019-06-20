using System;
using Microsoft.AspNetCore.Mvc;

public class FailController : Controller
{
    [HttpGet("/fail")]
    public IActionResult Error()
    {
        throw new Exception("Well, that escalated quickly.");
    }
}
