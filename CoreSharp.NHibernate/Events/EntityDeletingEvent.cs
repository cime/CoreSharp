﻿using CoreSharp.Cqrs.Events;
using CoreSharp.DataAccess;
using NHibernate;

namespace CoreSharp.NHibernate.Events
{
    public class EntityDeletingEvent<T> : IAsyncEvent
        where T : IEntity
    {
        public T Entity { get; }
        public ISession Session { get; }

        public EntityDeletingEvent(T entity, ISession session)
        {
            Entity = entity;
            Session = session;
        }
    }
}
