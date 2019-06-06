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

        public static bool IsBlank(this string input)
        {
            return String.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotBlank(this string input)
        {
            return !String.IsNullOrWhiteSpace(input);
        }

        public static string Truncate(this string value, int length)
        {
            if (String.IsNullOrEmpty(value))
                return value;
                
            return value.Length <= length
                ? value
                : value.Substring(0, length); 
        }        
    }
}
