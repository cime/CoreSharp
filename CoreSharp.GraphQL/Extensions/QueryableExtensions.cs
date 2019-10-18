using System.Linq.Dynamic.Core;
using CoreSharp.GraphQL.Models;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> queryable, Page page)
        {
            if (page != null)
            {
                queryable = queryable.Skip(page.Skip).Take(page.Take);
            }

            return queryable;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, Order order)
        {
            if (order != null)
            {
                var orderBy = order.Field + " " + order.Direction;
                queryable = queryable.OrderBy(orderBy);
            }

            return queryable;
        }
    }
}
