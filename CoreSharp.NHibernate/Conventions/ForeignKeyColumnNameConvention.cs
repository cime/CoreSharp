using System;
using CoreSharp.DataAccess;
using FluentNHibernate;
using FluentNHibernate.Conventions;

namespace CoreSharp.NHibernate.Conventions
{
    public class ForeignKeyColumnNameConvention : ForeignKeyConvention
    {
        protected override string GetKeyName(Member property, Type type)
        {
            if (property == null)
            {
                var isCodeList = typeof(ICodeList).IsAssignableFrom(type);
                return (isCodeList ? type.Name + "Code" : type.Name + "Id");
            }
            else
            {
                var isCodeList = typeof(ICodeList).IsAssignableFrom(property.PropertyType);
                return (isCodeList ? property.Name + "Code" : property.Name + "Id");
            }
        }
    }
}
