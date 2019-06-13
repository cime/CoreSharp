using CoreSharp.Common.Events;
using CoreSharp.DataAccess;
using NHibernate;

namespace CoreSharp.NHibernate.Events
{
    public class EntitySavingOrUpdatingEvent<T> : IAsyncEvent
        where T : IEntity
    {
        public T Entity { get; }
        public ISession Session { get; }

        public EntitySavingOrUpdatingEvent(T entity, ISession session)
        {
            Entity = entity;
            Session = session;
        }
    }
}
