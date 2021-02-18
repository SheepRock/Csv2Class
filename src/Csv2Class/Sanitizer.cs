using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Csv2Class
{
    class Sanitizer
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
            input = Regex.Replace(input, @"[^\p{L}\p{Nd}\p{Pc}\p{Cf}]", "");

            //Identifiers must start with a letter, or _
            if (!char.IsLetter(input[0]) && input[0] != '_')
            {
                input = "_" + input;
            }

            // Identifiers should not contain two consecutive _ characters. 
            //Those names are reserved for compiler generated identifiers.
            input = Regex.Replace(input, @"_{2,}", "_");

            return input;
        }

        public static string ToPascalCase(string input)
        {
            // "_" and "-" are valid identifiers, but we remove them to make the name more like the convention
            //of C# class and property names
            return Regex.Replace(input, @"(?<=(\b|\d|_))\p{Ll}", x => x.Value.ToUpper())
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_",""); 
        }
    }
}
