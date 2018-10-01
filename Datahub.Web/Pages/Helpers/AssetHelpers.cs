namespace Datahub.Web.Pages.Helpers
{
  public static class AssetHelpers
  {
    public static string GetIconName(string resourceType) {
        switch (resourceType) {
            case "publication": return "fa-file-alt";
            case "series": return "fa-clock";
            default: return "fa-table";
        }
    }
  }
}