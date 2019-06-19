using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly Container _container;
        private readonly ICqrsOptions _options;

        private readonly Dictionary<string, CommandInfo> _commandTypes;
        private readonly Dictionary<string, QueryInfo> _queryTypes;

        private static readonly string ContentType = "application/json; charset=utf-8";

        private dynamic DeserializeQuery(string json, Type queryType) => JsonConvert.DeserializeObject(json, queryType, _jsonSerializerSettings);
        private object DeserializeCommand(string json, Type commandType) => JsonConvert.DeserializeObject(json, commandType, _jsonSerializerSettings);

        public CqrsMiddleware(Container container, ICqrsOptions options)
        {
            _container = container;
            _options = options;
            _jsonSerializerSettings = options.GetJsonSerializerSettings();

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

            var commandData = await new StreamReader(context.Request.Body).ReadToEndAsync();
            dynamic command = DeserializeCommand(commandData, info.CommandType);

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

            var json = result is string ? result : JsonConvert.SerializeObject(result, _jsonSerializerSettings);

            context.Response.ContentType = ContentType;
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

            // GET operations get their data through the query string, while POST operations expect a JSON
            // object being put in the body.
            var queryData = context.Request.Method == HttpMethod.Get.Method
                ? SerializationHelpers.ConvertQueryStringToJson(context.Request.QueryString.Value)
                : await new StreamReader(context.Request.Body).ReadToEndAsync();

            var info = _queryTypes[path];
            var query = DeserializeQuery(queryData, info.QueryType);

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

            var json = result is string ? result : JsonConvert.SerializeObject(result, _jsonSerializerSettings);

            context.Response.ContentType = ContentType;
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
        public static IApplicationBuilder UseCqrs(this IApplicationBuilder builder, Container container)
        {
            return builder.UseMiddleware<CqrsMiddleware>();
        }
    }
}
