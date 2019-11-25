using System;
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
                return type.Name + "Id";
            }
            else
            {
                return property.Name + "Id";
            }
        }
    }
}
