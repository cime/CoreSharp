using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InclusiveBetweenAttribute : ValidationAttribute
    {
        public InclusiveBetweenAttribute()
        {
        }

        public InclusiveBetweenAttribute(object fromValue, object toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public object? From { get; set; }
        public object? To { get; set; }
    }
}
