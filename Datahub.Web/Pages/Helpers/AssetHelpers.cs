
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Datahub.Web.Models;

namespace Datahub.Web.Pages.Helpers
{
    public static class AssetHelpers
    {
        public static string GetIconName(string resourceType)
        {
            switch (resourceType)
            {
                case "publication": return "fa-file-alt";
                case "series": return "fa-clock";
                default: return "fa-table";
            }
        }

        public static string GetFileNameForDisplay(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return Path.GetFileName(uri.LocalPath);
            else
                return String.Empty;
        }

        public static string EnsureHttpsLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                var b = new UriBuilder(uri)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = -1 // default port for scheme
                };

                return b.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public static string GetFileExtensionForDisplay(string fileExtension)
        {
            if (fileExtension.IsNotBlank())
                return fileExtension.ToUpper();
            else
                return String.Empty;
        }

        public static IEnumerable<Keyword> FilterKnownBadKeywords(IEnumerable<Keyword> keywords)
        {
            return keywords.Where(k => k.Value.ToLowerInvariant() != "todo!");
        }
    }
}
