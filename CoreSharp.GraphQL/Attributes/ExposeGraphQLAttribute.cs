using System;
using System.Text.RegularExpressions;

namespace CoreSharp.GraphQL.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeGraphQLAttribute : Attribute
    {
        private static readonly Regex FieldNameRegex = new Regex("^[_A-Za-z][_0-9A-Za-z]*$", RegexOptions.Compiled);

        public string? FieldName { get; set; }
        public bool IsFieldNameSet => !string.IsNullOrEmpty(FieldName);

        public ExposeGraphQLAttribute()
        {

        }

        public ExposeGraphQLAttribute(string fieldName)
        {
            if (!FieldNameRegex.IsMatch(fieldName))
            {
                throw new ArgumentException("Field name contains invalid characters", nameof(fieldName));
            }

            FieldName = fieldName;
        }
    }
}
