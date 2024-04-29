using CommandLine;

using System;
using System.Collections.Generic;
using System.Text;

namespace Csv2Class
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input csv file")]
        public string CsvFile { get; set; }
        [Option('s', "separator", Required = false, HelpText = "(Default: Current culture list separator) Csv field separator")]
        public string Separator { get; set; }
        [Option('h', "header", Required = false, HelpText = "If the csv file has a header row")]
        public bool HasHeader { get; set; }
        [Option('p',"namespace", Required = false, HelpText = "Destination namespace. If not provided, there must be a .csproj in the current folder, and its name will be used as the namespace.")]
        public string Namespace { get; set; }
        [Option('o', "output-dir", Required = false, HelpText = "Output directory")]
        public string OutputDir { get; set; }
        [Option('c', "class-name", Required = false, HelpText = "Name of the class to generate")]
        public string ClassName { get; set; }
        [Option('t', "culture", Required = false,HelpText ="(Default: InvariantCulture) String code of the culture used to parse float point numbers and DateTime. Ex:en-US")]
        public string CultureCode { get; set; }
        
        [Option('d', "display-attribute", Required = false, HelpText = "Generate ASP.NET \"Display\" attribute")]
        public bool GenerateDisplay { get; set; }
        [Option('x', "index-attribute", Required = false, HelpText = "Generate CsvHelper \"Index\" attribute")]
        public bool GenerateIndex { get; set; }
        [Option('n', "name-attribute", Required = false, HelpText = "Generate CsvHelper \"Name\" attribute")]
        public bool GenerateColumnName { get; set; }
        [Option('m', "map", Required = false, HelpText = "Use CsvHelper class map instead of attributes")]
        public bool UseClassMaps { get; set; }
        [Option('e', "datetimeonly", Required = false, HelpText = "Allow using the new DateOnly and TimeOnly types when guessing the column type")]
        public bool UseDateTimeOnly { get; set; }

    }
}
