
using System;
using System.IO;

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

        public static string GetFileExtensionForDisplay(string fileExtension)
        {
            if (fileExtension.IsNotBlank())
                return fileExtension.ToUpper();
            else
                return String.Empty;
        }
    }
}
