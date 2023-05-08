using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace Csv2Class
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       if (!Initialize(o))
                       {
                           return;
                       }
                       ProcessCsv(o);
                   });
        }

        static bool Initialize(Options o)
        {
            //Find the namespace
            if (string.IsNullOrWhiteSpace(o.Namespace))
            {
                //EnumerateFiles has a limitation that it only take into account the first 3 characters after the dot
                //So we have to use FirstOrDefault to ensure that we get only files with the correct extension
                o.Namespace = Directory
                    .EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => x.ToUpper().EndsWith(".CSPROJ"));
                if(o.Namespace == null)
                {
                    Console.WriteLine("Could not file a project in current directory. Please, run again specifing the project file.");
                    return false;
                }
                o.Namespace = Path.GetFileNameWithoutExtension(o.Namespace).Replace(" ","_");
            }

            //Process the field separator
            o.Separator = o.Separator switch
            {
                null => CultureInfo.CurrentCulture.TextInfo.ListSeparator,
                "\\t" => "\t",
                _ => o.Separator
            };

            //Get the output directory
            o.OutputDir ??= Directory.GetCurrentDirectory();
            o.OutputDir = Path.GetFullPath(o.OutputDir);
            
            //Ensure we have a valid class name
            o.ClassName ??= Sanitizer.SanitizeIdentifier(Path.GetFileNameWithoutExtension(o.CsvFile));

            return true;
        }

        static void ProcessCsv(Options o)
        {
            //initialize the culture we will use
            var culture = o.CultureCode == null ? CultureInfo.InvariantCulture : new CultureInfo(o.CultureCode,false);
            CsvConfiguration config = new(culture)
            {
                Delimiter = o.Separator,                
                HasHeaderRecord = false,
                BadDataFound = null
            };
            //Where the headers will be stored
            string[] headers = null;
            //All records will be stored here for easy Linq processing
            List<string[]> records = new(); 
            //Definition of the csv file struct
            List<ColumnDefinition> columns = new();
            using (CsvReader csv = new(new StreamReader(o.CsvFile), config))
            {
                //Check if the first row is the header
                if (o.HasHeader)
                {
                    csv.Parser.Read();
                    headers = csv.Parser.Record;
                }
                //Populate our list with all the raw records
                while (csv.Parser.Read())
                {
                    records.Add(csv.Parser.Record);
                }
            }

            //If the csv doesn't contain a header, create a standard header
            if (headers == null)
            {
                headers = new string[records.First().Length];
                for(int i = 0; i < headers.Length; i++)
                {
                    headers[i] = $"Column {i}";
                }
            }

            //Create the columns
            for(int i = 0; i < headers.Length; i++)
            {
                columns.Add(ProcessColumn(culture, headers, records, i));

                // Check if already exists a property with that name, and add an index if exists
                // This could not have been done based in the column header because with the sanitation of the header
                // we may have a case where multiple columns with different headers end up with the same property name
                // or adding an index could cause a conflict with another property.
                int index = 2;
                string propertyName = columns[i].PropertyName; //stores the base name
                while (columns.Take(i).Any(x => x.PropertyName == columns[i].PropertyName))
                {
                    columns[i].PropertyName = $"{propertyName}_{index++}";
                }
            }
            
            CreateClass(o, columns);
        }


        private static ColumnDefinition ProcessColumn(CultureInfo culture, string[] headers, List<string[]> records, int i)
        {
            ColumnDefinition column = new()
            {
                Index = i,
                //The display name will be the header of the column
                DisplayName = headers[i],
                //Sanitize the column name, so it becomes the Property name
                PropertyName = Sanitizer.SanitizeIdentifier(headers[i])
            };

            //Check if there are duplicate column names and apply the NameIndex
            if (headers.Where(x => x == headers[i]).Count() > 1)
            {
                column.NameIndex = headers[..i].Where(x => x == headers[i]).Count();
            }

            //If any record is empty, the datatype is nullable
            bool nulable = records.Any(x => string.IsNullOrWhiteSpace(x[i].Trim('\"')));
            var nonEmptyRecords = records
                .Where(x => !string.IsNullOrWhiteSpace(x[i].Trim('\"')))
                .Select(x => x[i].Trim('\"'));

            //Try to guess the data type

            //If the entire column is blank, then we can't guess the data type, so we use string
            if (!nonEmptyRecords.Any())
            {
                column.PropertyType = "string";
            }
            else if (nonEmptyRecords.All(x => int.TryParse(x, out _)))
            {
                column.PropertyType = nulable ? "int?" : "int";
            }
            else if (nonEmptyRecords.All(x => long.TryParse(x, out _)))
            {
                column.PropertyType = nulable ? "long?" : "long";
            }
            else if (nonEmptyRecords.All(x => double.TryParse(x, NumberStyles.Float, culture, out _)))
            {
                column.PropertyType = nulable ? "double?" : "double";
            }
            else if (nonEmptyRecords.All(x => DateTime.TryParse(x, culture, DateTimeStyles.None, out _)))
            {
                column.PropertyType = nulable ? "DateTime?" : "DateTime";
            }
            else if (nonEmptyRecords.All(x => TimeSpan.TryParse(x, culture, out _)))
            {
                column.PropertyType = nulable ? "TimeSpan?" : "TimeSpan";
            }
            else if (nonEmptyRecords.All(x => bool.TryParse(x, out _)))
            {
                column.PropertyType = nulable ? "bool?" : "bool";
            }
            else //If we could find a proper data type, treat it as string
            {
                column.PropertyType = "string";
            }
            return column;
        }
    
        static void CreateClass(Options o, List<ColumnDefinition> columns)
        {
            Directory.CreateDirectory(o.OutputDir);
            var classFile = Path.Combine(o.OutputDir, $"{o.ClassName}.cs");
            using (StreamWriter sw = new(classFile))
            {
                //Using directives
                sw.WriteLine("using System;");
                if (o.GenerateDisplay)
                {
                    sw.WriteLine("using System.ComponentModel.DataAnnotations;");
                }
                if (!o.UseClassMaps && (o.GenerateIndex || o.GenerateColumnName))
                {
                    sw.WriteLine("using CsvHelper.Configuration.Attributes;");
                }
                //Namespace
                sw.WriteLine($@"namespace {o.Namespace}");
                sw.WriteLine("{");
                //Class
                sw.WriteLine($@"    class {o.ClassName}");
                sw.WriteLine("    {");
                //Properties
                foreach (var column in columns.OrderBy(x => x.Index))
                {
                    //Attributes
                    if (o.GenerateDisplay)
                    {
                        sw.WriteLine($"        [Display(Name = \"{column.DisplayName}\")]");
                    }
                    if (!o.UseClassMaps)
                    {
                        if (o.GenerateIndex)
                        {
                            sw.WriteLine($"        [Index({column.Index})]");
                        }
                        if (o.GenerateColumnName)
                        {
                            sw.WriteLine($"        [Name(\"{column.DisplayName}\")]");
                            //If there is more than one column with the same name, add the NameIndex Attribute
                            if (column.NameIndex.HasValue)
                            {
                                sw.WriteLine($"        [NameIndex({column.NameIndex.Value})]");
                            }
                        }
                    }
                    //Property
                    sw.WriteLine($"        public {column.PropertyType} {column.PropertyName} {{ get; set; }}");
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }

            //ClassMap
            if(o.UseClassMaps && (o.GenerateIndex || o.GenerateColumnName))
            {
                var mapFile = Path.Combine(o.OutputDir, $"{o.ClassName}Map.cs");
                using StreamWriter sw = new(mapFile);
                //Using directives
                sw.WriteLine("using System;");
                sw.WriteLine("using CsvHelper.Configuration;");
                //Namespace
                sw.WriteLine($@"namespace {o.Namespace}");
                sw.WriteLine("{");
                //class
                sw.WriteLine($@"    class {o.ClassName}Map : ClassMap<{o.ClassName}>");
                sw.WriteLine("    {");
                //Constructor
                sw.WriteLine($"        public {o.ClassName}Map()");
                sw.WriteLine("        {");
                //Maps
                foreach (var column in columns.OrderBy(x => x.Index))
                {
                    sw.Write($"            Map(m => m.{column.PropertyName})");
                    if (o.GenerateIndex)
                    {
                        sw.Write($".Index({column.Index})");
                    }
                    if (o.GenerateColumnName)
                    {
                        sw.Write($".Name(\"{column.DisplayName}\")");
                        //If there is more than one column with the same name, add the NameIndex
                        if (column.NameIndex.HasValue)
                        {
                            sw.Write($".NameIndex({column.NameIndex.Value})");
                        }
                    }
                    sw.WriteLine($";");
                }
                sw.WriteLine("        }");
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }
    }
}
