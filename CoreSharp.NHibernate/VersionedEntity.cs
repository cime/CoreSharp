using System;
using System.ComponentModel;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate
{
    public abstract class VersionedEntity : VersionedEntity<IUser>
    {

    }

    public abstract class VersionedEntity<TUser> : Entity, IVersionedEntity<TUser>
        where TUser : IUser
    {
        public virtual int Version { get; protected set; }
        public virtual DateTime CreatedDate { get; protected set; }
        public virtual DateTime? ModifiedDate { get; protected set; }

        private TUser _createdBy;
        public virtual TUser CreatedBy
        {
            get { return _createdBy; }
            protected set { ResetField(ref _createdBy, value, ref _isCreatedByIdSet); }
        }

        private long? _createdById;
        private bool _isCreatedByIdSet = false;
        [ReadOnly(true)]
        public virtual long? CreatedById
        {
            get
            {
                if (_isCreatedByIdSet) return _createdById;
                return CreatedBy == null ? default(long) : CreatedBy.Id;
            }
            set
            {
                _isCreatedByIdSet = true;
                _createdById = value;
            }
        }

        private TUser _modifiedBy;
        public virtual TUser ModifiedBy
        {
            get { return _modifiedBy; }
            protected set { ResetField(ref _modifiedBy, value, ref _isModifiedByIdSet); }
        }

        private long? _modifiedById;
        private bool _isModifiedByIdSet = false;
        [ReadOnly(true)]
        public virtual long? ModifiedById
        {
            get
            {
                if (_isModifiedByIdSet) return _modifiedById;
                return ModifiedBy == null ? default(long) : ModifiedBy.Id;
            }
            set
            {
                _isModifiedByIdSet = true;
                _modifiedById = value;
            }
        }

        private void ResetField<T>(ref T field, T value, ref bool synthIsSetField)
        {
            field = value;
            synthIsSetField = false;
        }
    }

    public abstract class VersionedEntity<TUser, TType> : Entity<TType>, IVersionedEntity<TUser>
        where TUser : IUser
    {
        public virtual int Version { get; protected set; }
        public virtual DateTime CreatedDate { get; protected set; }
        public virtual DateTime? ModifiedDate { get; protected set; }

        private TUser _createdBy;
        public virtual TUser CreatedBy
        {
            get { return _createdBy; }
            protected set { ResetField(ref _createdBy, value, ref _isCreatedByIdSet); }
        }

        private long? _createdById;
        private bool _isCreatedByIdSet = false;
        [ReadOnly(true)]
        public virtual long? CreatedById
        {
            get
            {
                if (_isCreatedByIdSet) return _createdById;
                return CreatedBy == null ? default(long) : CreatedBy.Id;
            }
            set
            {
                _isCreatedByIdSet = true;
                _createdById = value;
            }
        }

        private TUser _modifiedBy;
        public virtual TUser ModifiedBy
        {
            get { return _modifiedBy; }
            protected set { ResetField(ref _modifiedBy, value, ref _isModifiedByIdSet); }
        }

        private long? _modifiedById;
        private bool _isModifiedByIdSet = false;
        [ReadOnly(true)]
        public virtual long? ModifiedById
        {
            get
            {
                if (_isModifiedByIdSet) return _modifiedById;
                return ModifiedBy == null ? default(long) : ModifiedBy.Id;
            }
            set
            {
                _isModifiedByIdSet = true;
                _modifiedById = value;
            }
        }

        private void ResetField<T>(ref T field, T value, ref bool synthIsSetField)
        {
            field = value;
            synthIsSetField = false;
        }
    }

    public abstract class VersionedEntityWithoutUser : Entity, IVersionedEntity
    {
        public virtual int Version { get; protected set; }
        public virtual DateTime CreatedDate { get; protected set; }
        public virtual DateTime? ModifiedDate { get; protected set; }
    }

    public abstract class VersionedEntityWithoutUser<TType> : Entity<TType>, IVersionedEntity
    {
        public virtual int Version { get; protected set; }
        public virtual DateTime CreatedDate { get; protected set; }
        public virtual DateTime? ModifiedDate { get; protected set; }
    }
}
