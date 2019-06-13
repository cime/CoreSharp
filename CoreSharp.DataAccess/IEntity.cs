using System;

namespace CoreSharp.DataAccess
{
    public interface IEntity
    {
        bool IsTransient();

        object GetId();

        Type GetIdType();
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; }
    }
}
