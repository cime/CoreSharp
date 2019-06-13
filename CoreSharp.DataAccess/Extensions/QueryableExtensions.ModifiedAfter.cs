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
        public static IQueryable<TType> ModifiedAfter<TType, TId>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o =>
                (o.ModifiedDate != null && o.ModifiedDate > timestamp) ||
                (o.ModifiedDate == null && o.CreatedDate > timestamp));
        }

        public static IQueryable<TType> ModifiedAfter<TType>(this IQueryable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.ModifiedAfter<TType, long>(timestamp);
        }

        public static IEnumerable<TType> ModifiedAfter<TType>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<long>
        {
            return query.ModifiedAfter<TType, long>(timestamp);
        }

        public static IEnumerable<TType> ModifiedAfter<TType, TId>(this IEnumerable<TType> query, DateTime? timestamp)
            where TType : IVersionedEntity<TId>
        {
            if (!timestamp.HasValue)
            {
                return query;
            }

            return query.Where(o =>
                (o.ModifiedDate != null && o.ModifiedDate > timestamp) ||
                (o.ModifiedDate == null && o.CreatedDate > timestamp));
        }
    }
}
