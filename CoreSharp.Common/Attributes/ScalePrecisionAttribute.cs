using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ScalePrecisionAttribute : ValidationAttribute
    {
        /// <summary>
        /// Defaults to Precision = 12, Scale = 2
        /// </summary>
        public ScalePrecisionAttribute() : this(12, 2)
        {
            
        }

        public ScalePrecisionAttribute(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; set; }
        public int Scale { get; set; }
    }
}
