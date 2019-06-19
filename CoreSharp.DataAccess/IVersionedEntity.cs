using System;

namespace CoreSharp.DataAccess
{
    public interface IVersionedEntity : IEntity
    {
        int Version { get; }
        DateTime CreatedDate { get; }
        DateTime? LastModifiedDate { get; }
    }

    public interface IVersionedEntity<out TId> : IEntity<TId>, IVersionedEntity
    {

    }
}
