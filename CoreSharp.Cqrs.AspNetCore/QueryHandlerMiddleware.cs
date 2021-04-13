using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.AspNetCore.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CoreSharp.Cqrs.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class QueryHandlerMiddleware : IMiddleware
    {
        private readonly CqrsFormatterRegistry _registry;
        private readonly ICqrsOptions _options;

        private readonly Lazy<Dictionary<string, QueryInfo>> _queryTypes;

        private readonly ConcurrentDictionary<Type, dynamic> _deserializeMethods = new ConcurrentDictionary<Type, dynamic>();
        private static readonly MethodInfo CreateDeserializeLambdaMethodInfo = typeof(QueryHandlerMiddleware).GetMethod(nameof(CreateDeserializeLambda), BindingFlags.NonPublic | BindingFlags.Static);

        public QueryHandlerMiddleware(CqrsFormatterRegistry registry, ICqrsOptions options)
        {
            _registry = registry;
            _options = options;

            _queryTypes = new Lazy<Dictionary<string, QueryInfo>>(() => options.GetQueryTypes()
                .SelectMany(x => x.HttpMethods, (ci, method) => new {QueryInfo = ci, HttpMethod = method})
                .ToDictionary(
                    keySelector: (x) => $"{x.HttpMethod} {options.GetQueryPath(x.QueryInfo)}",
                    elementSelector: x => x.QueryInfo,
                    comparer: StringComparer.OrdinalIgnoreCase));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await HandleQuery(context, _options);
        }

        private static Func<ICqrsFormatter, HttpRequest, ValueTask<T>> CreateDeserializeLambda<T>()
        {
            var methodInfo = typeof(ICqrsFormatter)
                .GetMethod(nameof(ICqrsFormatter.DeserializeAsync), BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(typeof(T));
            var formatterParameter = Expression.Parameter(typeof(ICqrsFormatter), "formatter");
            var requestParameter = Expression.Parameter(typeof(HttpRequest), "request");
            var call = Expression.Call(formatterParameter, methodInfo, requestParameter);

            return Expression.Lambda<Func<ICqrsFormatter, HttpRequest, ValueTask<T>>>(call, formatterParameter, requestParameter).Compile();
        }

        private async Task HandleQuery(HttpContext context, ICqrsOptions options)
        {
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            if (!_queryTypes.Value.ContainsKey($"{method} {path}"))
            {
                throw new QueryNotFoundException($"Query '{method} {path}' not found");
            }

            var info = _queryTypes.Value[$"{method} {path}"];
            var exposeAttribute = info.QueryType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var deserializeMethod = _deserializeMethods.GetOrAdd(info.QueryType, (t) =>
            {
                var mi = CreateDeserializeLambdaMethodInfo.MakeGenericMethod(t);
                return mi.Invoke(null, null);
            });

            var query = await deserializeMethod(formatter, context.Request);

            dynamic handler = options.GetInstance(info.QueryHandlerType);

            context.Items[IOwinContextExtensions.ContextKey] = new CqrsContext(context.Request.Path.Value, path, CqrsType.Command, info.QueryHandlerType);

            dynamic result;

            if (info.IsAsync)
            {
                result = await handler.HandleAsync(query, context.RequestAborted);
            }
            else
            {
                result = handler.Handle(query);
            }

            string json = null;

            if (result != null)
            {
                json = result is string ? result : await formatter.SerializeAsync(result, context.Request);
            }

            context.Response.ContentType = formatter.ContentType;

            if (json != null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(json, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        }
    }

    public static class QueryHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseQueries(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<QueryHandlerMiddleware>();
        }
    }
}
