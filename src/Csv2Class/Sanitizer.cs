using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Csv2Class
{
    partial class Sanitizer
    {
        public static string SanitizeIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or whitespace", nameof(input));
            }
            //Convert to PascalCase
            input = ToPascalCase(input);

            //The rules for allowed identifier names are described in the link
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/identifier-names

            //Identifiers may contain Unicode letter characters, decimal digit characters, 
            //Unicode connecting characters, Unicode combining characters, or Unicode formatting characters.
            input = UnallowedUnicode().Replace(input, "");

            //Identifiers must start with a letter, or _
            if (!char.IsLetter(input[0]) && input[0] != '_')
            {
                input = "_" + input;
            }

            // Identifiers should not contain two consecutive _ characters. 
            //Those names are reserved for compiler generated identifiers.
            input = DoubleUnderscore().Replace(input, "_");

            return input;
        }

        public static string ToPascalCase(string input)
        {
            // "_" and "-" are valid identifiers, but we remove them to make the name more like the convention
            //of C# class and property names
            return FirstWordLowercaseLetter().Replace(input, x => x.Value.ToUpper())
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_",""); 
        }

        [GeneratedRegex("[^\\p{L}\\p{Nd}\\p{Pc}\\p{Cf}]")]
        private static partial Regex UnallowedUnicode();
        [GeneratedRegex("_{2,}")]
        private static partial Regex DoubleUnderscore();
        [GeneratedRegex("(?<=(\\b|\\d|_))\\p{Ll}")]
        private static partial Regex FirstWordLowercaseLetter();
    }
}
