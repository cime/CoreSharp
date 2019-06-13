using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GreaterThanAttribute : ComparisonAttribute
    {
        public GreaterThanAttribute() { }

        public GreaterThanAttribute(object value) : base(value)
        {
        }
    }
}
