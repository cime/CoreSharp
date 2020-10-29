using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate.PostgreSQL.Attributes;
using CoreSharp.NHibernate.PostgreSQL.Types;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class JsonConvention : AttributePropertyConvention<JsonAttribute>
    {
        protected override void Apply(JsonAttribute attribute, IPropertyInstance instance)
        {
            var type = typeof(JsonType<>).GetGenericTypeDefinition().MakeGenericType(instance.Property.PropertyType);
            instance.CustomType(type);
        }
    }

    public class JsonbConvention : AttributePropertyConvention<JsonbAttribute>
    {
        protected override void Apply(JsonbAttribute attribute, IPropertyInstance instance)
        {
            var type = typeof(JsonbType<>).GetGenericTypeDefinition().MakeGenericType(instance.Property.PropertyType);
            instance.CustomType(type);
        }
    }
}
