using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class CollectionConvention : IHasManyConvention, IHasManyToManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.AsSet();
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.AsSet();
        }
    }
}
