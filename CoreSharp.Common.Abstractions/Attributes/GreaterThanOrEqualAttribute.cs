using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GreaterThanOrEqualAttribute : ComparisonAttribute
    {
        public GreaterThanOrEqualAttribute() { }

        public GreaterThanOrEqualAttribute(object value) : base(value)
        {
        }
    }
}
