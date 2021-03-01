using System;

namespace CoreSharp.Validation.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreValidationAttributesAttribute : Attribute
    {
        public IgnoreValidationAttributesAttribute()
        {
        }

        public string[] Properties { get; set; }
    }
}
