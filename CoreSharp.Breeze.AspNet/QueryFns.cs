using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.Filters;

namespace CoreSharp.Breeze.AspNet
{
    public static class QueryFns
    {
        private static readonly Regex QueryRegex = new Regex(@"(\{.*})&?", RegexOptions.Compiled);

        public static IQueryable ExtractQueryable(HttpActionExecutedContext context)
        {
            object result;
            if (context.Response == null || !context.Response.TryGetContentValue(out result))
            {
                return null;
            }

            IQueryable queryable;
            if (result is IQueryable)
            {
                queryable = (IQueryable) result;
            }
            else if (result is IEnumerable)
            {
                try
                {
                    queryable = ((IEnumerable) result).AsQueryable();
                }
                catch
                {
                    throw new Exception(
                        "Unable to convert this endpoints IEnumerable to an IQueryable. Try returning an IEnumerable<T> instead of just an IEnumerable.");
                }
            }
            else
            {
                return null;
            }

            return queryable;
        }

        public static string ExtractAndDecodeQueryString(HttpActionExecutedContext context)
        {
            return ExtractAndDecodeQueryString(context.Request.RequestUri.Query);
        }

        public static string ExtractAndDecodeQueryString(string queryString)
        {
            var q = WebUtility.UrlDecode(queryString);
            if (q.Length == 0)
            {
                return null;
            }

            var match = QueryRegex.Match(q);
            if (match.Success)
            {
                q = match.Groups[1].Captures[0].Value;
                return q == "{}" ? null : q;
            }

            return null;
        }
    }
}
