using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate.Extensions
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Batch fetch entities
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TValue">Parameter type</typeparam>
        /// <param name="queryable">IQueryable<TEntity></param>
        /// <param name="property"></param>
        /// <param name="values"></param>
        /// <param name="batchSize">Batch size, default = 500</param>
        /// <returns></returns>
        public static IList<TEntity> BatchFetch<TEntity, TValue>(this IQueryable<TEntity> queryable, Expression<Func<TEntity, TValue>> property, IList<TValue> values, int batchSize = 500)
            where TEntity : class, IEntity
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var meProperty = property.Body as MemberExpression;
            if (meProperty == null || meProperty.Expression != property.Parameters[0] ||
                meProperty.Member.MemberType != MemberTypes.Property)
            {
                throw new Exception("property");
            }

            var entities = new List<TEntity>();
            var propertyName = meProperty.Member.Name;

            values = values.Distinct().ToList();

            for (var i = 0; i < values.Count; i += batchSize)
            {
                var pe = Expression.Parameter(typeof(TEntity));
                var me = Expression.Property(pe, propertyName);
                var ce = Expression.Constant(values.Skip(i).Take(batchSize));
                var call = Expression.Call(typeof (Enumerable), "Contains", new[] {me.Type}, ce, me);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(call, pe);

                var w = queryable.Where(lambda).ToList();
                if (w.Count > 0)
                {
                    entities = entities.Union(w).ToList();
                }
            }

            return entities;
        }
    }
}
