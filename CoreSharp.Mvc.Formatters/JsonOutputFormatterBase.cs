using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.RegularExpressions;

namespace CoreSharp.Mvc.Formatters
{
    public abstract class JsonOutputFormatterBase : IOutputFormatter
    {

        private readonly Regex[] _types = new string[] { "text/plain", "application/json", "text/json", @"application/.*\+json" }.
            Select(x => new Regex(x)).ToArray();

        private readonly string _formatterName;

        protected JsonOutputFormatterBase(string formatterName)
        {
            _formatterName = formatterName;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            var name = context.HttpContext.GetFormatterName();
            bool nameStatus = name == _formatterName;
            bool contentTypeStatus = _types.Sum(x => x.Matches(GetContentType(context)).Count) > 0;
            return nameStatus && contentTypeStatus;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var payload = GetPayload(context);
            var rspType = GetOutputContentType(context);
            context.HttpContext.Response.ContentType = rspType;
            await context.HttpContext.Response.WriteAsync(payload);
        }

        public abstract string GetPayload(OutputFormatterWriteContext context);

        private string GetContentType(OutputFormatterCanWriteContext context)
        {
            var val = (context.ContentType.HasValue ? context.ContentType.Value : context.HttpContext.Request?.ContentType) ?? "application/json";
            return val.ToLowerInvariant();
        }

        private string GetOutputContentType(OutputFormatterWriteContext context)
        {
            var reqType = GetContentType(context);
            var rspType = _types.Sum(x => x.Matches(reqType).Count) > 0 ? reqType : "application/json";
            if(rspType == "text/plain")
            {
                rspType = "application/json";
            }
            return rspType;
        }

    }
}
