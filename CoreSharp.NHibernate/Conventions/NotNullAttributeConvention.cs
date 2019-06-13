using System.Reflection;
using CoreSharp.Common.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class NotNullAttributeConvention : AttributePropertyConvention<NotNullAttribute>, IReferenceConvention
    {
        protected override void Apply(NotNullAttribute attribute, IPropertyInstance instance)
        {
            instance.Not.Nullable();
        }

        public void Apply(IManyToOneInstance instance)
        {
            var attribute = instance.Property.MemberInfo.GetCustomAttribute<NotNullAttribute>();
            if (attribute == null)
            {
                return;
            }
            instance.Not.Nullable();
        }
    }
}
