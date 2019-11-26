using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CodeListConfigurationAttribute : Attribute
    {
        public CodeListConfigurationAttribute()
        {
            IdLength = 20;
        }

        public int IdLength { get; set; }

        public int? NameLength { get; set; }
    }
}
