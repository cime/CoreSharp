using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Events;
using NHibernate;
using NHibernate.Event;

namespace CoreSharp.NHibernate.EventListeners
{
    public class PostEntityActionEventListener : IPostUpdateEventListener, IPostInsertEventListener, IPostDeleteEventListener
    {
        private readonly IEventPublisher _eventPublisher;

        public PostEntityActionEventListener(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        private Task PublishEventAsync(object entity, ISession session, PostEntityActionType actionType)
        {
            if (entity is IEntity iEntity)
            {
                var @event = new PostEntityActionAsyncEvent(iEntity, session, actionType);
                return _eventPublisher.PublishAsync(@event);
            }

            return Task.CompletedTask;
        }

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            return PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Update);
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Update).GetAwaiter().GetResult();
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            return PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Insert);
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Insert).GetAwaiter().GetResult();
        }

        public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            return PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Delete);
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            PublishEventAsync(@event.Entity, @event.Session, PostEntityActionType.Delete).GetAwaiter().GetResult();
        }
    }
}
