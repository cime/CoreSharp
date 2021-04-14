using System;
using System.Collections.Generic;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class HttpMethodAttribute : Attribute
    {
        public IEnumerable<string> HttpMethods { get; }

        public HttpMethodAttribute(IEnumerable<string> httpMethods)
        {
            HttpMethods = httpMethods;
        }
    }

    public class HttpGetAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "GET" };

        public HttpGetAttribute()
            : base(SupportedMethods)
        {
        }
    }

    public class HttpPostAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "POST" };

        public HttpPostAttribute()
            : base(SupportedMethods)
        {
        }
    }

    public class HttpPutAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "PUT" };

        public HttpPutAttribute()
            : base(SupportedMethods)
        {
        }
    }

    public class HttpDeleteAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "DELETE" };

        public HttpDeleteAttribute()
            : base(SupportedMethods)
        {
        }
    }

    public class HttpPatchAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "PATCH" };

        public HttpPatchAttribute()
            : base(SupportedMethods)
        {
        }
    }

    public class HttpHeadAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> SupportedMethods = new [] { "HEAD" };

        public HttpHeadAttribute()
            : base(SupportedMethods)
        {
        }
    }
}
