using System;

namespace CoreSharp.DataAccess
{
    public interface IVersionedEntity : IEntity
    {
        int Version { get; }

        DateTime CreatedDate { get; }

        DateTime? ModifiedDate { get; }

        //object CreatedBy { get; }

        //object LastModifiedBy { get; }
    }

    public interface IVersionedEntity<TUser> : IVersionedEntity
    {
        TUser CreatedBy { get; }

        TUser ModifiedBy { get; }
    }
}
