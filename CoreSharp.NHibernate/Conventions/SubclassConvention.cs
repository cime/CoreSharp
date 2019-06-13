using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class SubclassConvention : ISubclassConvention
    {
        public void Apply(ISubclassInstance instance)
        {
            var attribute = instance.EntityType.GetCustomAttribute<DiscriminatorValueAttribute>();

            if (attribute != null)
            {
                instance.DiscriminatorValue(attribute.Value);
            }
            else
            {
                instance.DiscriminatorValue(instance.EntityType.Name);
            }
        }
    }
}
