using System;
using System.Collections.Generic;
using System.Text;

namespace Csv2Class
{
    class ColumnDefinition
    {
        public string DisplayName { get; set; }
        public int Index { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
    }
}
