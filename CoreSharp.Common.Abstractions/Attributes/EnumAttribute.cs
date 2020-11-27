using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EnumAttribute : ValidationAttribute
    {
        public Type Type { get; }

        public EnumAttribute(Type type)
        {
            Type = type;
        }
    }
}
