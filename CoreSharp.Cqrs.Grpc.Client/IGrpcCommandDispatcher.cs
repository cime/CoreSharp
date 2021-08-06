using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public interface IGrpcCommandDispatcher : ICommandDispatcher
    {
        void Dispatch(ICommand command, GrpcCqrsCallOptions options);
        TResult Dispatch<TResult>(ICommand<TResult> command, GrpcCqrsCallOptions options);
        Task DispatchAsync(IAsyncCommand command, GrpcCqrsCallOptions options, CancellationToken cancellationToken = default);
        Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, GrpcCqrsCallOptions options, CancellationToken cancellationToken = default);
    }
}
