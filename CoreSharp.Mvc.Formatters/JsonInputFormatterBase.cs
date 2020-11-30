using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CoreSharp.Mvc.Formatters
{
    public abstract class JsonInputFormatterBase : IInputFormatter
    {
        private readonly ConcurrentDictionary<Type, string> _formatters = new ConcurrentDictionary<Type, string>();

        private readonly Regex[] _types = new string[] { "text/plain", "application/json", "text/json", @"application/.*\+json" }.
            Select(x => new Regex(x)).ToArray();

        private readonly string _formatterName;

        protected JsonInputFormatterBase(string formatterName)
        {
            _formatterName = formatterName;
        }

        private string GetFormatterName(Type type)
        {
            return _formatters.GetOrAdd(type, t => {
                var attr = t.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(ExposeAttribute)) as ExposeAttribute;
                if(attr == null)
                {
                    return null;
                }
                var val = string.IsNullOrWhiteSpace(attr.Formatter) ? HttpContextExtensions.DefaultFormatter : attr.Formatter;
                return val;
            });
        }

        public bool CanRead(InputFormatterContext context)
        {
            bool contentTypeStatus = _types.Sum(x => x.Matches(GetContentType(context)).Count) > 0;
            var formatterName = GetFormatterName(context.ModelType);
            if(formatterName == null)
            {
                return false;
            }
            bool nameStatus = formatterName == _formatterName;
            return contentTypeStatus && nameStatus;
        }

        private string GetContentType(InputFormatterContext context)
        {
            var val = (context.HttpContext.Request?.ContentType) ?? "application/json";
            return val.ToLowerInvariant();
        }

        public abstract Task<InputFormatterResult> ReadAsync(InputFormatterContext context);

    }
}
