namespace CoreSharp.Cqrs.Events
{
    public interface IEventSubscriber
    {
        void Subscribe<TEvent>(EventHandler<TEvent> handlerDelegate, short priority = default(short))
            where TEvent : IEvent;

        bool Unsubscribe<TEvent>(EventHandler<TEvent> handlerDelegate) where TEvent : IEvent;

        void Subscribe<TEvent>(AsyncEventHandler<TEvent> handlerDelegate, short priority = default(short))
            where TEvent : IAsyncEvent;

        bool Unsubscribe<TEvent>(AsyncEventHandler<TEvent> handlerDelegate) where TEvent : IAsyncEvent;
    }
}
