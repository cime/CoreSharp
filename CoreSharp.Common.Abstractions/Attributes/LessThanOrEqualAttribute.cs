using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LessThanOrEqualAttribute : ComparisonAttribute
    {
        public LessThanOrEqualAttribute() { }

        public LessThanOrEqualAttribute(object value) : base(value)
        {
            
        }
    }
}
