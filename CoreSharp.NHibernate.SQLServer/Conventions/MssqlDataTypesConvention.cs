using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate.SQLServer.Conventions
{
    //More info: http://nhforge.org/blogs/nhibernate/archive/2009/03/11/nhibernate-and-ms-sql-server-2008-date-time-datetime2-and-datetimeoffset.aspx
    public class MssqlDataTypesConvention : ISchemaConvention, IPropertyConvention
    {
        private readonly HashSet<string> _validDialects = new HashSet<string>
            {
                typeof (MsSql2012Dialect).FullName,
                typeof (MsSql2008Dialect).FullName
            };
        public bool CanApply(Dialect dialect)
        {
            return _validDialects.Contains(dialect.GetType().FullName);
        }

        public void ApplyBeforeExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
        }

        public void ApplyAfterExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
        }

        public void Apply(IPropertyInstance instance)
        {
            if (new[] {typeof (TimeSpan), typeof (TimeSpan?)}.Contains(instance.Property.PropertyType))
            {
                instance.CustomType("TimeAsTimeSpan");
            }

            /*
            if(new []{typeof(DateTime), typeof(DateTime?)}.Contains(instance.Property.PropertyType))
                instance.CustomType("datetime2");
            */

            if (new[] {typeof (DateTimeOffset), typeof (DateTimeOffset?)}.Contains(instance.Property.PropertyType))
            {
                instance.CustomType("datetimeoffset");
            }
        }
    }
}
