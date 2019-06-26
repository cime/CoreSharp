using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeAttribute : Attribute
    {
        public string? Uri { get; set; }
        public bool IsUriSet => !string.IsNullOrEmpty(Uri);

        public ExposeAttribute()
        {

        }

        public ExposeAttribute(string uri)
        {
            Uri = uri;
        }
    }
}
