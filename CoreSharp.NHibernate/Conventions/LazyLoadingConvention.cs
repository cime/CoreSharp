using System.Reflection;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace CoreSharp.NHibernate.Conventions
{
    public class LazyLoadingConvention : IReferenceConvention, IHasManyConvention, IHasOneConvention, IHasManyToManyConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            var field = instance.GetType().GetField("mapping", BindingFlags.Instance | BindingFlags.NonPublic);
            var impl = field.GetValue(instance) as ManyToOneMapping;
            if (impl != null)
            {
                var attrField = typeof(ManyToOneMapping).GetField("attributes", BindingFlags.Instance | BindingFlags.NonPublic);
                var attrs = attrField.GetValue(impl) as AttributeStore;
                if (attrs != null && attrs.Get("Lazy") != null)
                    return;//Do not set lazy if was set elsewhere
            }
            instance.LazyLoad();
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            var field = instance.GetType().GetField("mapping", BindingFlags.Instance | BindingFlags.NonPublic);
            var impl = field.GetValue(instance) as CollectionMapping;
            if (impl != null)
            {
                var attrField = typeof(CollectionMapping).GetField("attributes", BindingFlags.Instance | BindingFlags.NonPublic);
                var attrs = attrField.GetValue(impl) as AttributeStore;
                if (attrs != null && attrs.Get("Lazy") != null)
                    return;
            }
            instance.ExtraLazyLoad();
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.ExtraLazyLoad();
        }

        public void Apply(IOneToOneInstance instance)
        {
            instance.LazyLoad();
        }
    }
}
