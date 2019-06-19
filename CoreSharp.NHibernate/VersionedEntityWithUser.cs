using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate
{
    [Serializable]
    [Ignore]
    public abstract class VersionedEntityWithUser<TUser> : VersionedEntityWithUser<TUser, long>
    {
        public override bool IsTransient()
        {
            return Id <= 0; //Breeze will set this to a negative value
        }
    }

    [Serializable]
    [Ignore]
    public abstract class VersionedEntityWithUser<TUser, TType> : VersionedEntity<TType>, IVersionedEntityWithUser<TUser, TType>
    {
        public virtual TUser CreatedBy { get; protected set; }

        public virtual TUser LastModifiedBy { get; protected set; }

    }
}
