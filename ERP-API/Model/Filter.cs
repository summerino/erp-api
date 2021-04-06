using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace ERP_API.Model
{
    public class Filter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Keyword { get; set; }

        private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
        {
            {"eq", "="},
            {"neq", "!="},
            {"lt", "<"},
            {"lte", "<="},
            {"gt", ">"},
            {"gte", ">="},
            {"startswith", "StartsWith"},
            {"endswith", "EndsWith"},
            {"contains", "Contains"},
            {"doesnotcontain", "Contains"},
            {"isempty", ""},
            {"isnotempty", "!" },
            {"isnull", "="},
            {"isnotnull", "!="},
            {"isnullorempty", ""},
            {"isnotnullorempty", "!"}
        };
        
        public string ToExpression(Type type, IList<Filter> filters)
        {
            var index = filters.IndexOf(this);
            var comparison = Operators[Operator];

            var currentPropertyType = GetLastPropertyType(type, Field);

            switch (Operator)
            {
                case "doesnotcontain" when filters[index].Keyword.GetType().IsArray:
                    return $"!@{index}.{comparison}({Field})";
                case "doesnotcontain":
                    return $"!{Field}.{comparison}(@{index})";
                case "isempty":
                case "isnotempty":
                    return $"{Field} {comparison} String.Empty";
                case "isnull":
                case "isnotnull":
                    return $"{Field} {comparison} null";
                case "isnullorempty":
                case "isnotnullorempty":
                    return $"{comparison}String.IsNullOrEmpty({Field})";
            }

            if (comparison == "StartsWith" || comparison == "EndsWith" || comparison == "Contains")
            {
                return filters[index].Keyword.GetType().IsArray && comparison == "Contains"
                    ? $"@{index}.{comparison}({Field})"
                    : $"{Field}.{comparison}(@{index})";
            }

            return $"{Field} {comparison} @{index}";
        }

        internal Type GetLastPropertyType(Type type, string path)
        {
            Type currentType = type;

            // Searches for the public property with the specified name
            foreach (string propertyName in path.Split('.'))
            {
                var property = currentType.GetProperty(
                    propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                currentType = property?.PropertyType;
            }
            
            return currentType;
        }
    }
}
