using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : ValidationAttribute
    {
        public LengthAttribute()
        {
            
        }

        public LengthAttribute(int max)
        {
            Max = max;
        }
        
        public int Min { get; set; }

        public int Max { get; set; }

        public bool IsMinSet()
        {
            return Min != int.MinValue;
        }
    }
}
