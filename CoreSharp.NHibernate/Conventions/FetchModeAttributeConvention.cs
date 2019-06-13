using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.DataAccess.Enums;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class FetchModeAttributeConvention : IReferenceConvention, IHasManyConvention, IHasManyToManyConvention
    {

        public void Apply(IManyToOneInstance instance)
        {
            var attr = instance.Property.MemberInfo.GetCustomAttribute<FetchModeAttribute>();
            if(attr == null) return;

            switch (attr.Mode)
            {
                case FetchMode.Select:
                    instance.Fetch.Select();
                    break;
                case FetchMode.Join:
                    instance.Fetch.Join();
                    break;
                case FetchMode.SubSelect:
                    instance.Fetch.Subselect();
                    break;
            }
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            var attr = instance.Member.GetCustomAttribute<FetchModeAttribute>();
            if (attr == null) return;
            switch (attr.Mode)
            {
                case FetchMode.Select:
                    instance.Fetch.Select();
                    break;
                case FetchMode.Join:
                    instance.Fetch.Join();
                    break;
                case FetchMode.SubSelect:
                    instance.Fetch.Subselect();
                    break;
            }
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            var attr = instance.Member.GetCustomAttribute<FetchModeAttribute>();
            if (attr == null) return;
            switch (attr.Mode)
            {
                case FetchMode.Select:
                    instance.Fetch.Select();
                    break;
                case FetchMode.Join:
                    instance.Fetch.Join();
                    break;
                case FetchMode.SubSelect:
                    instance.Fetch.Subselect();
                    break;
            }
        }
    }
}
