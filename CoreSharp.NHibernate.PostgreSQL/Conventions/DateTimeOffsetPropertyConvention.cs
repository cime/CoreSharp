using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class DateTimeOffsetPropertyConvention : IPropertyConvention
    {
        public void Apply(IPropertyInstance instance)
        {
            if (new[] {typeof (DateTimeOffset), typeof (DateTimeOffset?)}.Contains(instance.Property.PropertyType))
            {
                instance.CustomType("timestamptz");
            }
        }
    }
}
