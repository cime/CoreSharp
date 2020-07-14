using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.AspNetCore.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CoreSharp.Cqrs.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CqrsMiddleware : IMiddleware
    {
        public static readonly string ContextKey = "CQRS";

        private readonly CqrsFormatterRegistry _registry;
        private readonly ICqrsOptions _options;

        private readonly Lazy<Dictionary<string, CommandInfo>> _commandTypes;
        private readonly Lazy<Dictionary<string, QueryInfo>> _queryTypes;

        private readonly ConcurrentDictionary<Type, dynamic> _deserializeMethods = new ConcurrentDictionary<Type, dynamic>();
        private static readonly MethodInfo CreateDeserializeLambdaMethodInfo = typeof(CqrsMiddleware).GetMethod(nameof(CreateDeserializeLambda), BindingFlags.NonPublic | BindingFlags.Static);

        public CqrsMiddleware(CqrsFormatterRegistry registry, ICqrsOptions options)
        {
            _registry = registry;
            _options = options;

            _commandTypes = new Lazy<Dictionary<string, CommandInfo>>(() => options.GetCommandTypes().ToDictionary(
                keySelector: options.GetCommandKey,
                elementSelector: type => type,
                comparer: StringComparer.OrdinalIgnoreCase));

            _queryTypes = new Lazy<Dictionary<string, QueryInfo>>(() => options.GetQueryTypes().ToDictionary(
                keySelector: options.GetQueryKey,
                elementSelector: info => info,
                comparer: StringComparer.OrdinalIgnoreCase));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method == HttpMethod.Post.Method && context.Request.Path.Value.StartsWith(_options.CommandsPath, StringComparison.OrdinalIgnoreCase))
            {
                await HandleCommand(context, _options);
            }
            else if (context.Request.Path.Value.StartsWith(_options.QueriesPath, StringComparison.OrdinalIgnoreCase))
            {
                await HandleQuery(context, _options);
            }
            else
            {
                await next(context);
            }
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

        private async Task HandleCommand(HttpContext context, ICqrsOptions options)
        {
            var path = options.GetCommandPath(context.Request.Path.Value);

            if (!_commandTypes.Value.ContainsKey(path))
            {
                throw new CommandNotFoundException($"Command '{path}' not found");
            }

            dynamic result = null;

            var info = _commandTypes.Value[path];
            var exposeAttribute = info.CommandType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var deserializeMethod = _deserializeMethods.GetOrAdd(info.CommandType, (t) =>
            {
                var mi = CreateDeserializeLambdaMethodInfo.MakeGenericMethod(t);
                return mi.Invoke(null, null);
            });

            dynamic command = await deserializeMethod(formatter, context.Request);

            dynamic handler = options.GetInstance(info.CommandHandlerType);
            context.Items[ContextKey] = new CqrsContext(context.Request.Path.Value, path, CqrsType.Command, info.CommandHandlerType);

            if (info.IsGeneric)
            {
                if (info.IsAsync)
                {
                    result = await handler.HandleAsync(command, context.RequestAborted);
                }
                else
                {
                    result = handler.Handle(command);
                }
            }
            else
            {
                if (info.IsAsync)
                {
                    await handler.HandleAsync(command, context.RequestAborted);
                }
                else
                {
                    handler.Handle(command);
                }
            }

            CloseSession();

            string json = null;

            if (result != null)
            {
                json = result is string ? result : await formatter.SerializeAsync(result, context.Request);
            }

            context.Response.ContentType = formatter.ContentType;

            if (json != null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await HttpResponseWritingExtensions.WriteAsync(context.Response, json, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        }

        private async Task HandleQuery(HttpContext context, ICqrsOptions options)
        {
            var path = options.GetQueryPath(context.Request.Path.Value);

            if (!_queryTypes.Value.ContainsKey(path))
            {
                throw new QueryNotFoundException($"Query '{path}' not found");
            }

            var info = _queryTypes.Value[path];
            var exposeAttribute = info.QueryType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var deserializeMethod = _deserializeMethods.GetOrAdd(info.QueryType, (t) =>
            {
                var mi = CreateDeserializeLambdaMethodInfo.MakeGenericMethod(t);
                return mi.Invoke(null, null);
            });

            dynamic query = await deserializeMethod(formatter, context.Request);

            dynamic handler = options.GetInstance(info.QueryHandlerType);

            context.Items[ContextKey] = new CqrsContext(context.Request.Path.Value, path, CqrsType.Command, info.QueryHandlerType);

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
                await HttpResponseWritingExtensions.WriteAsync(context.Response, json, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        }

        private void CloseSession()
        {
            var session = (global::NHibernate.ISession) _options.GetInstance(typeof(global::NHibernate.ISession));

            if (session == null || !session.IsOpen)
            {
                return;
            }

            if (session.GetSessionImplementation().TransactionInProgress)
            {
                var tx = session.Transaction;
                try
                {
                    if (tx.IsActive) tx.Commit();
                    session.Close();
                }
                catch (Exception)
                {
                    if (tx.IsActive) tx.Rollback();
                    throw;
                }
            }
            else
            {
                session.Close();
            }
        }
    }

    public static class CqrsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCqrs(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CqrsMiddleware>();
        }
    }
}
