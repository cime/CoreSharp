using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class LazyLoadAttributeConvention : AttributePropertyConvention<LazyLoadAttribute>, IReferenceConvention, IHasManyConvention, IHasManyToManyConvention
    {
        protected override void Apply(LazyLoadAttribute attribute, IPropertyInstance instance)
        {
            if (attribute.Enabled)
                instance.LazyLoad();
            else
                instance.Not.LazyLoad();
        }

        public void Apply(IManyToOneInstance instance)
        {
            var attr = instance.Property.MemberInfo.GetCustomAttribute<LazyLoadAttribute>();
            if (attr == null) return;
            if (attr.Enabled)
                instance.LazyLoad();
            else
                instance.Not.LazyLoad();
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            var attr = instance.Member.GetCustomAttribute<LazyLoadAttribute>();
            if (attr == null) return;
            if (attr.Enabled)
                instance.LazyLoad();
            else
                instance.Not.LazyLoad();
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            var attr = instance.Member.GetCustomAttribute<LazyLoadAttribute>();
            if (attr == null) return;
            if (attr.Enabled)
                instance.LazyLoad();
            else
                instance.Not.LazyLoad();
        }
    }
}
