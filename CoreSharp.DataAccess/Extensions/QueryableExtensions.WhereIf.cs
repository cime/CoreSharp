using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace CoreSharp.DataAccess.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, bool condition)
        {
            return !condition ? source : source.Where(predicate);
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, string value)
        {
            return string.IsNullOrEmpty(value) ? source : source.Where(predicate);
        }

        public static IQueryable<TSource> WhereIf<TSource, TType>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, TType? value)
            where TType : struct
        {
            return !value.HasValue ? source : source.Where(predicate);
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, ICollection value)
        {
            return value == null || value.Count == 0 ? source : source.Where(predicate);
        }
    }
}
