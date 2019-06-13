using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LessThanAttribute : ComparisonAttribute
    {
        public LessThanAttribute() { }

        public LessThanAttribute(object value) : base(value)
        {
        }
    }
}
