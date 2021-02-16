using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class GrpcClientCommandDispatcher : IGrpcCommandDispatcher
    {

        private readonly IGrpcClientManager _clientManager;

        public GrpcClientCommandDispatcher(IGrpcClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void Dispatch(ICommand command, GrpcCqrsCallOptions options)
        {
            throw new NotImplementedException();
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command, GrpcCqrsCallOptions options)
        {
            var rsp = _clientManager.GetClientFor(command).Execute<ICommand<TResult>, TResult>(command, options, default).Result;
            return rsp.Value;
        }

        public Task DispatchAsync(IAsyncCommand command, GrpcCqrsCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, GrpcCqrsCallOptions options, CancellationToken cancellationToken)
        {
            var rsp = await _clientManager.GetClientFor(command).Execute<IAsyncCommand<TResult>, TResult>(command, options, cancellationToken);
            return rsp.Value;
        }

        #region ICommandDispatcher

        public void Dispatch(ICommand command)
        {
            Dispatch(command, null);
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command)
        {
            return Dispatch(command, null);
        }

        public async Task DispatchAsync(IAsyncCommand command)
        {
            await DispatchAsync(command, null, default);
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command)
        {
            return await DispatchAsync(command, null, default);
        }

        public async Task DispatchAsync(IAsyncCommand command, CancellationToken cancellationToken)
        {
            await DispatchAsync(command, null, cancellationToken);
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, CancellationToken cancellationToken)
        {
            return await DispatchAsync(command, null, cancellationToken);
        }

        #endregion

    }
}
