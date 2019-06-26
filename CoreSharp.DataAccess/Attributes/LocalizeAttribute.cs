using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class LocalizeAttribute : Attribute
    {
        public string? PropName { get; set; }
        public string? PropType { get; set; }

        public LocalizeAttribute(string propName, string propType)
        {
            PropName = propName;
            PropType = propType;
        }

        public LocalizeAttribute() { }
    }
}
