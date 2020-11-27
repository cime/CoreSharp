using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeAttribute : Attribute
    {
        public string? Uri { get; set; }
        public bool IsUriSet => !string.IsNullOrEmpty(Uri);

        public string Formatter { get; set; }

        public ExposeAttribute()
        {

        }

        public ExposeAttribute(string uri)
        {
            Uri = uri;
        }

        public ExposeAttribute(string uri, string formatter)
        {
            Uri = uri;
            Formatter = formatter;
        }
    }
}
