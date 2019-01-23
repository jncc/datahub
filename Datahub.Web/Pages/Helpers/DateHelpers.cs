using System;
using System.Linq;

namespace Datahub.Web.Pages.Helpers
{
    public static class DateHelpers
    {
        public static string[] SplitIso8601DateIfPossible(this string iso8601Date)
        {
            DateTime parsed;

            if (DateTime.TryParse(iso8601Date, out parsed))
            {
                return new [] { parsed.ToString("yyyy"), parsed.ToString("MM"), parsed.ToString("dd")};
            }
            else
            {
                return new [] {iso8601Date, "", ""};
            }
        }
    }
}
