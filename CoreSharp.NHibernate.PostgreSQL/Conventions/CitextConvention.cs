using System;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate.PostgreSQL.Attributes;
using CoreSharp.NHibernate.PostgreSQL.Types;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class CitextConvention : IPropertyConvention
    {
        private static readonly Type StringType = typeof(string);

        public void Apply(IPropertyInstance instance)
        {
            if (instance.Property.PropertyType == StringType && instance.EntityType.GetCustomAttribute<LengthAttribute>() == null &&  instance.SqlType == null)
            {
                instance.CustomSqlType("text");
                instance.CustomType<CitextType>();
            }

            if (instance.Property.MemberInfo.GetCustomAttribute<CitextAttribute>() != null)
            {
                instance.CustomSqlType("citext");
                instance.CustomType<CitextType>();

                return;
            }

            if (instance.Property.Name != "Id" && instance.EntityType.GetCustomAttribute<CitextAttribute>() != null)
            {
                instance.CustomSqlType("citext");
                instance.CustomType<CitextType>();
            }
        }
    }
}
