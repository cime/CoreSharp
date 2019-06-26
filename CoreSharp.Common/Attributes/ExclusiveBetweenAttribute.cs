using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExclusiveBetweenAttribute : ValidationAttribute
    {
        public ExclusiveBetweenAttribute()
        {
        }

        public ExclusiveBetweenAttribute(object fromValue, object toValue)
        {
            From = fromValue;
            To = toValue;
        }

        public object? From { get; set; }
        public object? To { get; set; }
    }
}
