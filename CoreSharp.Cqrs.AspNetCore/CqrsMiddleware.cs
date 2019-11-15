using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleInjector;

namespace CoreSharp.Cqrs.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CqrsMiddleware : IMiddleware
    {
        public static readonly string ContextKey = "CQRS";

        private readonly Container _container;
        private readonly CqrsFormatterRegistry _registry;
        private readonly ICqrsOptions _options;

        private readonly Dictionary<string, CommandInfo> _commandTypes;
        private readonly Dictionary<string, QueryInfo> _queryTypes;

        public CqrsMiddleware(Container container, CqrsFormatterRegistry registry, ICqrsOptions options)
        {
            _container = container;
            _registry = registry;
            _options = options;

            _commandTypes = options.GetCommandTypes().ToDictionary(
                keySelector: options.GetCommandKey,
                elementSelector: type => type,
                comparer: StringComparer.OrdinalIgnoreCase);

            _queryTypes = options.GetQueryTypes().ToDictionary(
                keySelector: options.GetQueryKey,
                elementSelector: info => info,
                comparer: StringComparer.OrdinalIgnoreCase);
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

        private async Task HandleCommand(HttpContext context, ICqrsOptions options)
        {
            var path = options.GetCommandPath(context.Request.Path.Value);

            if (!_commandTypes.ContainsKey(path))
            {
                throw new CommandNotFoundException($"Command '{path}' not found");
            }

            dynamic result = null;

            var info = _commandTypes[path];
            var exposeAttribute = info.CommandType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var command = await formatter.DeserializeAsync(context.Request, info.CommandType);

            dynamic handler = _container.GetInstance(info.CommandHandlerType);
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

            var json = result is string ? result : await formatter.SerializeAsync(result);

            context.Response.ContentType = formatter.ContentType;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await HttpResponseWritingExtensions.WriteAsync(context.Response, json, context.RequestAborted);
        }

        private async Task HandleQuery(HttpContext context, ICqrsOptions options)
        {
            var path = options.GetQueryPath(context.Request.Path.Value);

            if (!_queryTypes.ContainsKey(path))
            {
                throw new QueryNotFoundException($"Query '{path}' not found");
            }

            var info = _queryTypes[path];
            var exposeAttribute = info.QueryType.GetCustomAttribute<ExposeAttribute>();
            var formatter = _registry.GetFormatter(exposeAttribute.Formatter);

            var query = await formatter.DeserializeAsync(context.Request, info.QueryType);

            dynamic handler = _container.GetInstance(info.QueryHandlerType);

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

            var json = result is string ? result : await formatter.SerializeAsync(result);

            context.Response.ContentType = formatter.ContentType;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await HttpResponseWritingExtensions.WriteAsync(context.Response, json, context.RequestAborted);
        }

        private void CloseSession()
        {
            var session = _container.GetInstance<global::NHibernate.ISession>();

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
