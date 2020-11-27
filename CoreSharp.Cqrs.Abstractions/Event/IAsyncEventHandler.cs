using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Events
{
    /// <summary>
    /// Event handler for <typeparam name="TEvent" />
    /// </summary>
    /// <typeparam name="TEvent"> command</typeparam>
    public interface IAsyncEventHandler<in TEvent> where TEvent : IAsyncEvent
    {
        /// <summary>
        /// Handle <typeparam name="TEvent" /> event
        /// </summary>
        /// <param name="event"><typeparamref name="TEvent"/> event</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
