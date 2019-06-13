using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullOrEmptyAttribute : ValidationAttribute
    {
    }
}
