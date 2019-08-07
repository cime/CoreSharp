using CoreSharp.Common.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class DefaultValueAttributeConvention : AttributePropertyConvention<DefaultValueAttribute>
    {
        protected override void Apply(DefaultValueAttribute attribute, IPropertyInstance instance)
        {
            if (!string.IsNullOrEmpty(attribute.Value))
            {
                if(instance.Type == typeof(bool) || instance.Type == typeof(bool?))
                {
                    instance.Default(attribute.Value == "1" ? "true" : "false");
                }
                else
                {
                    instance.Default(attribute.Value);
                }
            }
        }
    }
}
