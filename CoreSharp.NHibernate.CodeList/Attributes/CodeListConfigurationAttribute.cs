using System;

namespace CoreSharp.NHibernate.CodeList.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CodeListConfigurationAttribute : Attribute
    {
        public CodeListConfigurationAttribute()
        {
            CodeLength = 20;
            CodeListPrefix = true;
        }

        public int CodeLength { get; set; }

        public int? NameLength { get; set; }

        public bool CodeListPrefix { get; set; }
    }
}
