using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Command
{
    public interface IAsyncCommandHandler<in TCommand> where TCommand : IAsyncCommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken);
    }

    public interface IAsyncCommandHandler<in TCommand, TResult> where TCommand : IAsyncCommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
