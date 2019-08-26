using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }

        public bool View { get; set; }
    }
}
