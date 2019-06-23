using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using CoreSharp.NHibernate.Events;
using NHibernate.Event;

namespace CoreSharp.NHibernate.EventListeners
{
    public class DeleteEntityEventListener : IDeleteEventListener
    {
        private readonly IEventPublisher _eventPublisher;

        public DeleteEntityEventListener(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public void OnDelete(DeleteEvent @event)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(@event.Entity.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();
        }

        public void OnDelete(DeleteEvent @event, ISet<object> transientEntities)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(@event.Entity.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();
        }

        public async Task OnDeleteAsync(DeleteEvent @event, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(@event.Entity.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);
        }

        public async Task OnDeleteAsync(DeleteEvent @event, ISet<object> transientEntities, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(@event.Entity.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);
        }
    }
}
