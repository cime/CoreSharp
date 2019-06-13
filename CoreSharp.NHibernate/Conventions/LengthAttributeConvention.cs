using CoreSharp.Common.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    class LengthAttributeConvention : AttributePropertyConvention<LengthAttribute>
    {
        protected override void Apply(LengthAttribute attribute, IPropertyInstance instance)
        {
            //http://stackoverflow.com/questions/2343105/override-for-fluent-nhibernate-for-long-text-strings-nvarcharmax-not-nvarchar
            if (instance.Type != typeof(byte[]))
            {
                instance.Length(attribute.Max == int.MaxValue ? 10000 : attribute.Max);
            }
            else
            {
                instance.Length(attribute.Max);
            }
        }
    }
}
