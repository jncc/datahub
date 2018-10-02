using System;
using System.Linq;

namespace Datahub.Web.Pages.Helpers
{
  public static class StringHelpers
  {
      public static string FirstCharToUpper(this string input)
      {
          switch (input)
          {
              case null: throw new ArgumentNullException(nameof(input));
              case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
              default: return input.First().ToString().ToUpper() + input.Substring(1);
          }
      }
  }
}
