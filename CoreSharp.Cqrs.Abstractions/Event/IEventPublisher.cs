using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Events
{
    public interface IEventPublisher
    {
        void Publish(IEvent @event);

        Task PublishAsync(IAsyncEvent @event);

        Task PublishAsync(IAsyncEvent @event, CancellationToken cancellationToken);
    }
}
