using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

#nullable disable

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    public class IndexAttributeConvention : AttributePropertyConvention<IndexAttribute>, IReferenceConvention
    {
        protected override void Apply(IndexAttribute attribute, IPropertyInstance instance)
        {
            instance.Index(attribute.IsKeySet
                ? GetIndexName(instance.EntityType.Name, attribute.KeyName)
                : GetIndexName(instance.EntityType.Name, instance.Name));
        }

        public void Apply(IManyToOneInstance instance)
        {
            var attribute = instance.Property.MemberInfo.GetCustomAttribute<IndexAttribute>();

            if (attribute == null)
            {
                return;
            }

            instance.Index(attribute.IsKeySet
                ? GetIndexName(instance.EntityType.Name, attribute.KeyName)
                : GetIndexName(instance.EntityType.Name, instance.Name));
        }

        private static string GetIndexName(string tableName, string name)
        {
            return $"ix_{tableName}_{name}";
        }
    }
}
