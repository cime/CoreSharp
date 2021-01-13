using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Dispatch(ICommand command)
        {
            throw new NotImplementedException();
        }

        public Task DispatchAsync(IAsyncCommand command)
        {
            throw new NotImplementedException();
        }

        public Task DispatchAsync(IAsyncCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command)
        {
            var rsp = _clientManager.GetClientFor(command).Execute<ICommand<TResult>, TResult>(command, default).Result;
            return rsp.Value;
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command)
        {
            var rsp = await _clientManager.GetClientFor(command).Execute<IAsyncCommand<TResult>, TResult>(command, default);
            return rsp.Value;
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, CancellationToken cancellationToken)
        {
            var rsp = await _clientManager.GetClientFor(command).Execute<IAsyncCommand<TResult>, TResult>(command, cancellationToken);
            return rsp.Value;
        }
    }
}
