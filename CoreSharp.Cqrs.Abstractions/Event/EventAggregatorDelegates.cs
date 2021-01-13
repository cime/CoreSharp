using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Events
{
    public delegate void EventHandler<in TEvent>(TEvent @event) where TEvent : IEvent;
    public delegate Task AsyncEventHandler<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IAsyncEvent;
}
