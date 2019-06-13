using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExactLengthAttribute : ValidationAttribute
    {
        public ExactLengthAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; private set; }
    }
}
