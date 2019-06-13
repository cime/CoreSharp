using System;

namespace CoreSharp.Validation.Attributes
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
