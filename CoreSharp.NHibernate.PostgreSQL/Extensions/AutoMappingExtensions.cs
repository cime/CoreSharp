using System;
using System.Linq.Expressions;
using CoreSharp.NHibernate.PostgreSQL.Types;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace CoreSharp.NHibernate.PostgreSQL.Extensions
{
    public static class AutoMappingExtensions
    {
        public static PropertyPart MapCitext<T>(this AutoMapping<T> mapping, Expression<Func<T, object>> memberExpression)
        {
            var propertyPart = mapping.Map(memberExpression);
            propertyPart.CustomSqlType("citext");
            propertyPart.CustomType<CitextType>();

            return propertyPart;
        }
    }
}
