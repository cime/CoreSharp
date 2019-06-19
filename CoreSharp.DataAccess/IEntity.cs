using System;

namespace CoreSharp.DataAccess
{
    public interface IEntity
    {
        object GetId();
        bool IsTransient();
    }

    public interface IEntity<out TId> : IEntity
    {
        TId Id { get; }
    }
}
