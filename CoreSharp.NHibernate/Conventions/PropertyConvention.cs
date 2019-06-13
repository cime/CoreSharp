using System;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class PropertyConvention : IPropertyConvention
    {
        public void Apply(IPropertyInstance instance)
        {
            if (new[] {typeof (DateTime), typeof (DateTime?)}.Contains(instance.Property.PropertyType))
            {
                instance.CustomType("UtcDateTime");
            }
        }
    }
}
