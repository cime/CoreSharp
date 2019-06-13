using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotEqualAttribute : ComparisonAttribute
    {
        public NotEqualAttribute() { }

        public NotEqualAttribute(object value) : base(value)
        {
        }
    }
}
