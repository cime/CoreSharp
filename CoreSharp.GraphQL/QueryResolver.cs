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

        public QueryResolver(Container container, Type queryHandlerType, Type queryType)
        {
            _container = container;
            _queryHandlerType = queryHandlerType;
            _queryType = queryType;
        }

        public object Resolve(ResolveFieldContext context)
        {
            var queryHandler = _container.GetInstance(_queryHandlerType);
            // TODO: find a better way to deserialize variable query
            var variableValue = context.Variables.SingleOrDefault(x => x.Name == "query")?.Value;
            var query =
                JsonConvert.DeserializeObject(JsonConvert.SerializeObject(variableValue), _queryType);

            var isAsync = _queryHandlerType.IsAssignableToGenericType(typeof(IQueryHandler<,>));
            
            var handleMethodInfo =
                _queryHandlerType.GetMethod(isAsync ? "HandleAsync" : "Handle", BindingFlags.Instance | BindingFlags.Public);

            return handleMethodInfo.Invoke(queryHandler, new[] {query});
        }
    }
}
