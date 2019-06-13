using System;
using System.Linq.Expressions;
using FluentNHibernate.Mapping;

// ReSharper disable once CheckNamespace
namespace FluentNHibernate.Automapping
{
    public static class OneToManyPartExtensions
    {
        /// <summary>
        /// Specify the key column name using the given expression with additional "Id" postfix
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="oneToMany"></param>
        /// <param name="columnExp"></param>
        /// <returns></returns>
        public static OneToManyPart<TChild> KeyColumn<TChild>(this OneToManyPart<TChild> oneToMany, Expression<Func<TChild, object>> columnExp)
        {
            return oneToMany.KeyColumn(columnExp.GetFullPropertyName() + "Id");
        }

        public static OneToManyPart<TChild> PropertyRef<TChild>(this OneToManyPart<TChild> oneToMany, Expression<Func<TChild, object>> columnExp)
        {
            return oneToMany.PropertyRef(columnExp.GetFullPropertyName());
        }
    }
}
