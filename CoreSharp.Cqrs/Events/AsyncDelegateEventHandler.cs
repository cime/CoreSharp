using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Events
{
    internal class AsyncDelegateEventHandler<TEvent> : DelegateEventHandler,
            IAsyncEventHandler<TEvent> where TEvent : IAsyncEvent
    {
        private readonly AsyncEventHandler<TEvent> _handler;

        public AsyncDelegateEventHandler(AsyncEventHandler<TEvent> handler, short priority) : base(priority)
        {
            _handler = handler;
        }

        public Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            return _handler(@event, cancellationToken);
        }
    }
}
