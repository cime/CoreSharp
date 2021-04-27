using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CoreSharp.Cqrs.Query;
using GraphQL;
using GraphQL.Resolvers;
using Newtonsoft.Json;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class QueryResolver : IFieldResolver
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object, IResolveFieldContext, object>> Cache = new ConcurrentDictionary<Type, Func<object, object, IResolveFieldContext, object>>();

        private readonly Container _container;
        private readonly Type _queryHandlerType;
        private readonly Type _queryType;
        private readonly Type _resultType;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public QueryResolver(Container container, Type queryHandlerType, Type queryType, Type resultType,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _container = container;
            _queryHandlerType = queryHandlerType;
            _queryType = queryType;
            _resultType = resultType;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public object Resolve(IResolveFieldContext context)
        {
            var queryHandler = _container.GetInstance(_queryHandlerType);
            var variableValue = context.Arguments?.ContainsKey("query") == true ? JsonConvert.SerializeObject(context.Arguments["query"].Value, _jsonSerializerSettings) : "{}";
            var query = JsonConvert.DeserializeObject(variableValue, _queryType, _jsonSerializerSettings);

            if (query != null)
            {
                var contextProperties = query.GetType().GetProperties().Where(x => typeof(IResolveFieldContext).IsAssignableFrom(x.PropertyType) && x.CanWrite).ToList();

                foreach (var contextProperty in contextProperties)
                {
                    contextProperty.SetValue(query, context);
                }
            }


            var handleMethodInfo = Cache.GetOrAdd(_queryHandlerType, (type) =>
            {
                var p1 = Expression.Parameter(typeof(object), "queryHandler");
                var p2 = Expression.Parameter(typeof(object), "query");
                var p3 = Expression.Parameter(typeof(IResolveFieldContext), "context");

                var isAsync = type.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>));

                MethodCallExpression call;
                if (isAsync)
                {
                    var mi = _queryHandlerType.GetMethod(nameof(IAsyncQueryHandler<IAsyncQuery<object>, object>.HandleAsync), BindingFlags.Instance | BindingFlags.Public, null, new []{ _queryType, typeof(CancellationToken) }, null);
                    var cn = Expression.Property(p3, "CancellationToken");

                    call = Expression.Call(Expression.Convert(p1, _queryHandlerType), mi, Expression.Convert(p2, _queryType), cn);
                }
                else
                {
                    var mi = _queryHandlerType.GetMethod(nameof(IQueryHandler<IQuery<object>, object>.Handle), BindingFlags.Instance | BindingFlags.Public, null, new []{ _queryType }, null);
                    call = Expression.Call(Expression.Convert(p1, _queryHandlerType), mi, Expression.Convert(p2, _queryType));
                }

                return Expression.Lambda<Func<object, object, IResolveFieldContext, object>>(
                    Expression.Convert(call, typeof(object)),
                    p1, p2, p3).Compile();
            });

            return handleMethodInfo(queryHandler, query, context);
        }
    }
}
