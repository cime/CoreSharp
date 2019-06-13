using System;
using System.Collections.Generic;
using CoreSharp.DataAccess;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class VersionedEntityConvention : IPropertyConvention, IReferenceConvention
    {
        private static readonly Type TypeOfIVersionedEntity = typeof(IVersionedEntity);
        private static readonly Type TypeOfIUser = typeof(IUser);
        private static readonly IList<string> NotNullableProperties = new List<string>() { "CreatedBy", "CreatedDate" };
        private static readonly IList<string> NonCascadableProperties = new List<string>() { "CreatedBy", "ModifiedBy" };

        public void Apply(IPropertyInstance instance)
        {
            if (TypeOfIVersionedEntity.IsAssignableFrom(instance.EntityType) && !TypeOfIUser.IsAssignableFrom(instance.EntityType))
            {
                if (NotNullableProperties.Contains(instance.Property.Name))
                {
                    instance.Not.Nullable();
                }
            }
        }

        public void Apply(IManyToOneInstance instance)
        {
            if (TypeOfIVersionedEntity.IsAssignableFrom(instance.EntityType))
            {
                if (NonCascadableProperties.Contains(instance.Property.Name))
                {
                    instance.Cascade.None();
                }
            }
        }
    }
}
