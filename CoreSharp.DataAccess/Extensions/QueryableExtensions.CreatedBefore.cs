using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace CoreSharp.DataAccess.Extensions
{
    /// <summary>
    /// Queryable extensions
    /// </summary>
    public static partial class QueryableExtensions
    {
        public static IQueryable<TType> CreatedBefore<TType, TId>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o => o.CreatedDate < timestamp);
        }

        public static IQueryable<TType> CreatedBefore<TType>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.CreatedAfter<TType, long>(timestamp);
        }

        public static IEnumerable<TType> CreatedBefore<TType>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.CreatedBefore<TType, long>(timestamp);
        }

        public static IEnumerable<TType> CreatedBefore<TType, TId>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o => o.CreatedDate < timestamp);
        }
    }
}
