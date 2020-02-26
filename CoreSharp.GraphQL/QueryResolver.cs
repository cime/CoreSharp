using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Query;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class QueryResolver : IFieldResolver
    {
        private readonly Container _container;
        private readonly Type _queryHandlerType;
        private readonly Type _queryType;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public QueryResolver(Container container, Type queryHandlerType, Type queryType,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _container = container;
            _queryHandlerType = queryHandlerType;
            _queryType = queryType;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public object Resolve(IResolveFieldContext context)
        {
            var queryHandler = _container.GetInstance(_queryHandlerType);
            var variableValue = context.Arguments?.ContainsKey("query") == true ? JsonConvert.SerializeObject(context.Arguments["query"], _jsonSerializerSettings) : "{}";
            var query = JsonConvert.DeserializeObject(variableValue, _queryType, _jsonSerializerSettings);

            if (query != null)
            {
                var contextProperties = query.GetType().GetProperties().Where(x => typeof(IResolveFieldContext).IsAssignableFrom(x.PropertyType) && x.CanWrite).ToList();

                foreach (var contextProperty in contextProperties)
                {
                    contextProperty.SetValue(query, context);
                }
            }

            var isAsync = _queryHandlerType.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>));

            var handleMethodInfo =
                _queryHandlerType.GetMethod(isAsync ? "HandleAsync" : "Handle", BindingFlags.Instance | BindingFlags.Public);

            return handleMethodInfo.Invoke(queryHandler, new[] {query});
        }
    }
}
