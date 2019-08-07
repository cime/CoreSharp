using CoreSharp.NHibernate.CodeList.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.CodeList.Conventions
{
    public class FilterCurrentLanguageAttributeConvention : AttributePropertyConvention<FilterCurrentLanguageAttribute>
    {
        protected override void Apply(FilterCurrentLanguageAttribute attribute, IPropertyInstance instance)
        {
            // The real formula will be set in the NHMappings event listener.
            // We need to set the formula here in order to prevent fluent nh to create a column mapping.
            instance.Formula("Id");
        }
    }
}
