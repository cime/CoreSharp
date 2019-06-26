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

        public object Resolve(ResolveFieldContext context)
        {
            var queryHandler = _container.GetInstance(_queryHandlerType);
            var variableValue = context.Arguments["query"];
            var query =
                JsonConvert.DeserializeObject(JsonConvert.SerializeObject(variableValue, _jsonSerializerSettings), _queryType, _jsonSerializerSettings);

            var isAsync = _queryHandlerType.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>));

            var handleMethodInfo =
                _queryHandlerType.GetMethod(isAsync ? "HandleAsync" : "Handle", BindingFlags.Instance | BindingFlags.Public);

            return handleMethodInfo.Invoke(queryHandler, new[] {query});
        }
    }
}
