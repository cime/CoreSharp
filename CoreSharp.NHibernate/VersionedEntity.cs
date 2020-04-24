using System;
using System.ComponentModel;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate
{
    [Serializable]
    [Ignore]
    public abstract class VersionedEntity : VersionedEntity<long>
    {
        public override bool IsTransient()
        {
            return Id <= 0; //Breeze will set this to a negative value
        }
    }

    [Serializable]
    [Ignore]
    public abstract class VersionedEntity<TId> :  Entity<TId>, IVersionedEntity<TId>, IEntityState
    {
        private bool _isTransient = true;

        public virtual int Version { get; protected set; }

        public virtual DateTime CreatedDate { get; protected set; }

        public virtual DateTime? LastModifiedDate { get; protected set; }

        public override bool IsTransient()
        {
            return _isTransient;
        }

        public virtual void SetTransient(bool isTransient)
        {
            _isTransient = isTransient;
        }
    }
}
