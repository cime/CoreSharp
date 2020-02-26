using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleInjector;

namespace CoreSharp.GraphQL.AspNetCore
{
    public class GraphQLMiddleware<TSchema> : IMiddleware
        where TSchema : CqrsSchema
    {
        private const string JsonContentType = "application/json";
        private const string GraphQLContentType = "application/graphql";
        private const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

        private readonly Container _container;
        private readonly ILogger<GraphQLMiddleware<TSchema>> _logger;
        private readonly TSchema _schema;
        private readonly IComplexityConfigurationFactory _complexityConfigurationFactory;
        private readonly IUserContextBuilder _userContextBuilder;

        private readonly PathString _path = "/graphql";

        public GraphQLMiddleware(
            Container container,
            ILogger<GraphQLMiddleware<TSchema>> logger,
            TSchema schema, IComplexityConfigurationFactory complexityConfigurationFactory,
            IUserContextBuilder userContextBuilder)
        {
            _container = container;
            _logger = logger;
            _schema = schema;
            _complexityConfigurationFactory = complexityConfigurationFactory;
            _userContextBuilder = userContextBuilder;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.WebSockets.IsWebSocketRequest || !context.Request.Path.StartsWithSegments(_path))
            {
                await next(context);
                return;
            }

            // Handle requests as per recommendation at http://graphql.org/learn/serving-over-http/
            var httpRequest = context.Request;
            var gqlRequest = new GraphQLRequest();
            var documentWriter = new DocumentWriter(_schema.GetJsonSerializerSettings());

            if (HttpMethods.IsGet(httpRequest.Method) || (HttpMethods.IsPost(httpRequest.Method) && httpRequest.Query.ContainsKey(GraphQLRequest.QueryKey)))
            {
                ExtractGraphQLRequestFromQueryString(httpRequest.Query, gqlRequest);
            }
            else if (HttpMethods.IsPost(httpRequest.Method))
            {
                if (!MediaTypeHeaderValue.TryParse(httpRequest.ContentType, out var mediaTypeHeader))
                {
                    await WriteBadRequestResponseAsync(context, documentWriter, $"Invalid 'Content-Type' header: value '{httpRequest.ContentType}' could not be parsed.");
                    return;
                }

                switch (mediaTypeHeader.MediaType)
                {
                    case JsonContentType:
                        gqlRequest = Deserialize<GraphQLRequest>(httpRequest.Body);
                        break;
                    case GraphQLContentType:
                        gqlRequest.Query = await ReadAsStringAsync(httpRequest.Body);
                        break;
                    case FormUrlEncodedContentType:
                        var formCollection = await httpRequest.ReadFormAsync();
                        ExtractGraphQLRequestFromPostBody(formCollection, gqlRequest);
                        break;
                    default:
                        await WriteBadRequestResponseAsync(context, documentWriter, $"Invalid 'Content-Type' header: non-supported media type. Must be of '{JsonContentType}', '{GraphQLContentType}', or '{FormUrlEncodedContentType}'. See: http://graphql.org/learn/serving-over-http/.");
                        return;
                }
            }

            IEnumerable<IValidationRule> validationRules;
            try
            {
                validationRules = _container.GetAllInstances<IValidationRule>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                validationRules = new IValidationRule[0];
            }

            var executer = new DocumentExecuter();
            var result = await executer.ExecuteAsync(x =>
            {
                x.Schema = _schema;
                x.OperationName = gqlRequest.OperationName;
                x.Query = gqlRequest.Query;
                x.Inputs = gqlRequest.GetInputs();
                x.UserContext = _userContextBuilder.BuildContext();
                x.ValidationRules = validationRules.Concat(DocumentValidator.CoreRules);
                x.CancellationToken = context.RequestAborted;
                x.ComplexityConfiguration = _complexityConfigurationFactory.GetComplexityConfiguration();
            });

            if (result.Errors?.Any(x => x.Code == "auth-required") == true && context.Response.Headers.ContainsKey("Token-Expired"))
            {
                await WriteUnauthorizedResponseAsync(context, documentWriter, result);
            }
            else
            {
                await WriteResponseAsync(context, documentWriter, result);
            }
        }


        private Task WriteBadRequestResponseAsync(HttpContext context, IDocumentWriter writer, string errorMessage)
        {
            var result = new ExecutionResult()
            {
                Errors = new ExecutionErrors()
                {
                    new ExecutionError(errorMessage)
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return writer.WriteAsync(context.Response.Body, result);
        }

        private Task WriteUnauthorizedResponseAsync(HttpContext context, IDocumentWriter writer, ExecutionResult result)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return writer.WriteAsync(context.Response.Body, result);
        }

        private Task WriteResponseAsync(HttpContext context, IDocumentWriter writer, ExecutionResult result)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            return writer.WriteAsync(context.Response.Body, result);
        }

        private static T Deserialize<T>(Stream s)
        {
            using (var reader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return new JsonSerializer().Deserialize<T>(jsonReader);
            }
        }

        private static async Task<string> ReadAsStringAsync(Stream s)
        {
            using (var reader = new StreamReader(s))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static void ExtractGraphQLRequestFromQueryString(IQueryCollection qs, GraphQLRequest gqlRequest)
        {
            gqlRequest.Query = qs.TryGetValue(GraphQLRequest.QueryKey, out var queryValues) ? queryValues[0] : null;
            gqlRequest.Variables = qs.TryGetValue(GraphQLRequest.VariablesKey, out var variablesValues) ? JObject.Parse(variablesValues[0]) : null;
            gqlRequest.OperationName = qs.TryGetValue(GraphQLRequest.OperationNameKey, out var operationNameValues) ? operationNameValues[0] : null;
        }

        private static void ExtractGraphQLRequestFromPostBody(IFormCollection fc, GraphQLRequest gqlRequest)
        {
            gqlRequest.Query = fc.TryGetValue(GraphQLRequest.QueryKey, out var queryValues) ? queryValues[0] : null;
            gqlRequest.Variables = fc.TryGetValue(GraphQLRequest.VariablesKey, out var variablesValue) ? JObject.Parse(variablesValue[0]) : null;
            gqlRequest.OperationName = fc.TryGetValue(GraphQLRequest.OperationNameKey, out var operationNameValues) ? operationNameValues[0] : null;
        }
    }

    public static class GraphQLMiddlewareExtensions
    {
        public static IApplicationBuilder UseGraphQL<TSchema>(this IApplicationBuilder builder)
            where TSchema : CqrsSchema
        {
            return builder.UseMiddleware<GraphQLMiddleware<TSchema>>();
        }
    }
}
