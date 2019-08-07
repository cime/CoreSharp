using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class DateTimeOffsetPropertyConvention : IPropertyConvention, ISchemaConvention
    {
        private readonly HashSet<string> _validDialects = new HashSet<string>
        {
            typeof (PostgreSQLDialect).FullName,
            typeof (PostgreSQL81Dialect).FullName,
            typeof (PostgreSQL82Dialect).FullName,
            typeof (PostgreSQL83Dialect).FullName
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
            if (new[] {typeof (DateTimeOffset), typeof (DateTimeOffset?)}.Contains(instance.Property.PropertyType))
            {
                instance.CustomType("timestamptz");
            }
        }
    }
}
