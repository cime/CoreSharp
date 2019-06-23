using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.AspNetCore
{
    [DebuggerDisplay("{QueryType.Name,nq}")]
    public sealed class QueryInfo
    {
        public readonly Type QueryType;
        public readonly Type QueryHandlerType;
        public readonly Type ResultType;
        public readonly bool IsAsync;

        public QueryInfo(Type queryType)
        {
            QueryType = queryType;
            IsAsync = queryType.IsAssignableToGenericType(typeof(IAsyncQuery<>));
            ResultType = DetermineResultTypes(queryType, IsAsync).Single();

            if (IsAsync)
            {
                QueryHandlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(QueryType, ResultType);
            }
            else
            {
                QueryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(QueryType, ResultType);
            }
        }

        private static IEnumerable<Type> DetermineResultTypes(Type type, bool isAsync) =>
            from interfaceType in type.GetInterfaces()
            where interfaceType.IsGenericType
            where (!isAsync && interfaceType.GetGenericTypeDefinition() == typeof(IQuery<>)) ||
                  (isAsync && interfaceType.GetGenericTypeDefinition() == typeof(IAsyncQuery<>))
            select interfaceType.GetGenericArguments()[0];
    }
}
