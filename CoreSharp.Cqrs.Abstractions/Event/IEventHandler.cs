namespace CoreSharp.Cqrs.Events
{
    /// <summary>
    /// Event handler for <typeparam name="TEvent" />
    /// </summary>
    /// <typeparam name="TEvent"> command</typeparam>
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Handle <typeparam name="TEvent" /> event
        /// </summary>
        /// <param name="event"><typeparamref name="TEvent"/> event</param>
        void Handle(TEvent @event);
    }
}
