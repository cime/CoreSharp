using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Command
{
    public interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
        TResult Dispatch<TResult>(ICommand<TResult> command);
        Task DispatchAsync(IAsyncCommand command);
        Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command);
        Task DispatchAsync(IAsyncCommand command, CancellationToken cancellationToken);
        Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, CancellationToken cancellationToken);
    }
}
