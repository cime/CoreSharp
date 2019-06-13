using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CodeListAttribute : Attribute
    {
        public int CodeLength { get; set; }

        public CodeListAttribute()
        {
            CodeLength = 20;
        }
    }
}
