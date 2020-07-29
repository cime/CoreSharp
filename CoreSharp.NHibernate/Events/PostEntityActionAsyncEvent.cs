using CoreSharp.Cqrs.Events;
using CoreSharp.DataAccess;
using NHibernate;

namespace CoreSharp.NHibernate.Events
{
    public enum PostEntityActionType
    {
        Insert,
        Update,
        Delete
    }

    public class PostEntityActionAsyncEvent : IAsyncEvent
    {
        public IEntity Entity { get; }
        public ISession Session { get; }
        public PostEntityActionType ActionType { get; }

        public PostEntityActionAsyncEvent(IEntity entity, ISession session, PostEntityActionType actionType)
        {
            Entity = entity;
            Session = session;
            ActionType = actionType;
        }
    }
}
