using System;
using System.Collections;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreSharp.Breeze.AspNetCore
{
    public static class QueryFns
    {
        public static IQueryable ExtractQueryable(ActionExecutedContext context)
        {
            var objResult = context.Result as ObjectResult;
            if (objResult == null) return null;

            var result = objResult.Value;
            IQueryable queryable = null;

            if (result is IQueryable)
            {
                queryable = (IQueryable) result;
            }
            else if (result is string)
            {
                return null;
            }
            else if (result is IEnumerable)
            {
                try
                {
                    queryable = ((IEnumerable) result).AsQueryable();
                }
                catch
                {
                    throw new Exception("Unable to convert this endpoints IEnumerable to an IQueryable. Try returning an IEnumerable<T> instead of just an IEnumerable.");
                }
            }
            else
            {
                throw new Exception("Unable to convert this endpoint to an IQueryable");
            }

            return queryable;
        }

        public static string ExtractAndDecodeQueryString(ActionContext context)
        {
            if (!context.HttpContext.Request.Query.ContainsKey("query"))
            {
                return null;
            }

            var qs = context.HttpContext.Request.Query["query"];
            var q = WebUtility.UrlDecode(qs);

            if (q?.Length == 0)
            {
                return null;
            }

            return q;
        }
    }
}
