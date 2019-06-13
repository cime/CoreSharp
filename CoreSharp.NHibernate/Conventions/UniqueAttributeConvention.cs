using System.Linq;
using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class UniqueAttributeConvention : AttributePropertyConvention<UniqueAttribute>, IReferenceConvention
    {
        protected override void Apply(UniqueAttribute attribute, IPropertyInstance instance)
        {
            if (attribute.IsKeySet)
            {
                var keys = attribute.KeyName.Split(',').Select(x => $"UQ_{x.Trim()}");

                instance.UniqueKey(string.Join(",", keys));
            }
            else
            {
                instance.Unique();
            }
        }

        public void Apply(IManyToOneInstance instance)
        {
            var attribute = instance.Property.MemberInfo.GetCustomAttribute<UniqueAttribute>();
            if (attribute == null)
            {
                return;
            }

            if (attribute.IsKeySet)
            {
                var keys = attribute.KeyName.Split(',').Select(x => $"UQ_{x.Trim()}");

                instance.UniqueKey(string.Join(",", keys));
            }
            else
            {
                instance.Unique();
            }
        }
    }
}
