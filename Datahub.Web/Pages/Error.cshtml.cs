using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;


namespace Datahub.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private ILogger<ErrorModel> Logger { get; set; }

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            Logger = logger;
        }

        public void OnGet()
        {
            IExceptionHandlerPathFeature exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null) {
                Exception ex = exceptionFeature.Error;
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                Logger.LogError("{RequestId}: {Message}", RequestId, ex.ToString());
            } 
        }
    }
}
