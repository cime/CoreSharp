using System;

namespace CoreSharp.Common.Attributes
{
    public abstract class ValidationAttribute : Attribute
    {
        /// <summary>
        /// Whether include property name in error message
        /// </summary>
        public bool IncludePropertyName { get; set; }
    }
}
