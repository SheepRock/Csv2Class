# Csv2Class
Small tool to scaffold a CSV file into a C# class, with options to add the Display attribute from ASP.NET MVC and the Index and Name attributes from CsvHelper

# Instalation 
```shell
dotnet tool install --global Csv2Class --version 1.2.0
```

# Release notes

## 1.2.0
 * If multiple columns have the same header, adds the NameIndex attribute to differentiate
 * Fixes a bug where multiple columns could end up with the same property name, after sanitation

## 1.1.0
 * Fixes a bug where an empty column would end up with int? type instead of string

## 1.0.0
 * First Release
 
# Usage

## Basic usage
Navigate to the directory of your .csproj and run the tool passing the source csv file to scaffold
```bat
csv2class -i myCsvFilePath.csv
```

By default, it assumes that your csv don't have a header, and the columns and properties will be named in sequence "Column 0", "Column 1"...
If your csv has a header row, add the `-h` switch. The header is used to get the name of the properties and are used in the `Display` attribute from ASP.NET MVC and `Name` attribute from CsvHelper
```bat
csv2class -i myCsvFilePath.csv -h
```

The default delimiter is the current culture list separator from your system. To change that, use the `-s` option
```bat
csv2class -i myCsvFilePath.csv -s ;
csv2class -i myCsvFilePath.csv -s \t
csv2class -i myCsvFilePath.csv -s ,
```

The csproj file is used to guess the namespace. As an alternative, you can explicit set the namespace of the generated classes:
```bat
csv2class -i myCsvFilePath.csv -p MyNamespace
```

The csv file name will be used for the class name. You can specify your own class name with the `-c` option
```bat
csv2class -i myCsvFilePath.csv -c MyClass
```

To specify the output directory where the files will be saved, use the `-o` option. The directory will be created, if it doesn't exist.
```bat
csv2class -i myCsvFilePath.csv -o OutputDir
```

To guess the double and DateTime data types, it is used the Invariant Culture. To specify a culture, use the `-t` option
```bat
csv2class -i myCsvFilePath.csv -t en-US 
```

To generate the ASP.NET MVC `Display` attribute, use the `-d` switch
```bat
csv2class -i myCsvFilePath.csv -d 
```

To generate the CsvHelper `Index` and `Name` attributes, use the `-x` and/or `n` switches, respectively
```bat
csv2class -i myCsvFilePath.csv -x
csv2class -i myCsvFilePath.csv -n
csv2class -i myCsvFilePath.csv -xn
```

The default is to generate CsvHelper attributes. If you wish to use the ClassMap intead, use the `-m` switch. It will be generate a file in the same folder of the class file, with the name {Class}Map
```bat
csv2class -i myCsvFilePath.csv -xm
csv2class -i myCsvFilePath.csv -n -m
csv2class -i myCsvFilePath.csv -xnm
```


## All options
```
  -i, --input                Required. Input csv file

  -s, --separator            (Default: Current culture list separator) Csv field separator

  -h, --header               If the csv file has a header row

  -p, --namespace            Destination namespace. If not provided, there must be a .csproj in the current folder, and
                             its name will be used as the namespace.

  -o, --output-dir           Output directory

  -c, --class-name           Name of the class to generate

  -t, --culture              (Default: InvariantCulture) String code of the culture used to parse float point numbers
                             and DateTime. Ex:en-US

  -d, --display-attribute    Generate ASP.NET "Display" attribute

  -x, --index-atrribute      Generate CsvHelper "Index" attribute

  -n, --name-atrribute       Generate CsvHelper "Name" attribute

  -m, --map                  Use CsvHelper class map instead of attributes

  --help                     Display this help screen.
  ```
