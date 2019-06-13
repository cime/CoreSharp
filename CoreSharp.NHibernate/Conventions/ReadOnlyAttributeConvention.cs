using System.ComponentModel;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class ReadOnlyAttributeConvention : AttributePropertyConvention<ReadOnlyAttribute>
    {
        protected override void Apply(ReadOnlyAttribute attribute, IPropertyInstance instance)
        {
            if (attribute.IsReadOnly)
            {
                instance.ReadOnly();
            }
            else
            {
                instance.Not.ReadOnly();
            }
        }
    }
}
