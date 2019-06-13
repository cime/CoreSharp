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
        public static IQueryable<TType> CreatedAfter<TType, TId>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o => o.CreatedDate > timestamp);
        }

        public static IQueryable<TType> CreatedAfter<TType>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.CreatedAfter<TType, long>(timestamp);
        }

        public static IEnumerable<TType> CreatedAfter<TType>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.CreatedAfter<TType, long>(timestamp);
        }

        public static IEnumerable<TType> CreatedAfter<TType, TId>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o => o.CreatedDate > timestamp);
        }
    }
}
