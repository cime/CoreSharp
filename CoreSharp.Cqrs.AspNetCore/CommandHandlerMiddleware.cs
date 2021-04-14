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
using NHibernate;

namespace CoreSharp.Cqrs.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CommandHandlerMiddleware : IMiddleware
    {
        private readonly CqrsFormatterRegistry _registry;
        private readonly ICqrsOptions _options;

        private readonly Lazy<Dictionary<string, CommandInfo>> _commandTypes;

        private readonly ConcurrentDictionary<Type, dynamic> _deserializeMethods = new ConcurrentDictionary<Type, dynamic>();
        private static readonly MethodInfo CreateDeserializeLambdaMethodInfo = typeof(CommandHandlerMiddleware).GetMethod(nameof(CreateDeserializeLambda), BindingFlags.NonPublic | BindingFlags.Static);

        public CommandHandlerMiddleware(CqrsFormatterRegistry registry, ICqrsOptions options)
        {
            _registry = registry;
            _options = options;

            _commandTypes = new Lazy<Dictionary<string, CommandInfo>>(() =>
                options.GetCommandTypes()
                    .SelectMany(x => x.HttpMethods, (ci, method) => new { CommandInfo = ci, HttpMethod = method})
                    .ToDictionary(
                        keySelector: (x) => $"{x.HttpMethod} {options.GetCommandPath(x.CommandInfo)}",
                        elementSelector: x => x.CommandInfo,
                        comparer: StringComparer.OrdinalIgnoreCase));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await HandleCommand(context, _options);
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
            var path = context.Request.Path;
            var method = context.Request.Method;

            if (!_commandTypes.Value.ContainsKey($"{method} {path}"))
            {
                throw new CommandNotFoundException($"Command '{method} {path}' not found");
            }

            dynamic result = null;

            var info = _commandTypes.Value[$"{method} {path}"];
            var exposeAttribute = info.CommandType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var deserializeMethod = _deserializeMethods.GetOrAdd(info.CommandType, (t) =>
            {
                var mi = CreateDeserializeLambdaMethodInfo.MakeGenericMethod(t);
                return mi.Invoke(null, null);
            });

            var command = await deserializeMethod(formatter, context.Request);

            dynamic handler = options.GetInstance(info.CommandHandlerType);
            context.Items[IOwinContextExtensions.ContextKey] = new CqrsContext(context.Request.Path.Value, path, CqrsType.Command, info.CommandHandlerType);

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

        private void CloseSession()
        {
            var session = (global::NHibernate.ISession) _options.GetInstance(typeof(global::NHibernate.ISession));

            if (session == null || !session.IsOpen)
            {
                return;
            }

            if (session.GetSessionImplementation().TransactionInProgress)
            {
                var tx = session.GetCurrentTransaction();
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

    public static class CommandHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCommands(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CommandHandlerMiddleware>();
        }
    }
}
