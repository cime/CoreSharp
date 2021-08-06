using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class CommandDispatcherRouteDecorator : IGrpcCommandDispatcher
    {
        private readonly IGrpcClientManager _clientManager;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IGrpcCommandDispatcher _grpcDispatcher;

        public CommandDispatcherRouteDecorator(IGrpcClientManager clientManager, ICommandDispatcher dispatcher, IGrpcCommandDispatcher grpcDispatcher)
        {
            _clientManager = clientManager;
            _dispatcher = dispatcher;
            _grpcDispatcher = grpcDispatcher;
        }

        public void Dispatch(ICommand command, GrpcCqrsCallOptions options)
        {
            var remoteDispatcher = GetRemoteDispatcher(command);
            if (remoteDispatcher != null)
            {
                remoteDispatcher.Dispatch(command, options);
            }
            else
            {
                _dispatcher.Dispatch(command);
            }
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command, GrpcCqrsCallOptions options)
        {
            var remoteDispatcher = GetRemoteDispatcher(command);
            if (remoteDispatcher != null)
            {
                return remoteDispatcher.Dispatch(command, options);
            }
            else
            {
                return _dispatcher.Dispatch(command);
            }
        }

        public async Task DispatchAsync(IAsyncCommand command, GrpcCqrsCallOptions options, CancellationToken cancellationToken)
        {
            var remoteDispatcher = GetRemoteDispatcher(command);
            if(remoteDispatcher != null)
            {
                await remoteDispatcher.DispatchAsync(command, options, cancellationToken);
            } else
            {
                await _dispatcher.DispatchAsync(command, cancellationToken);
            }
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, GrpcCqrsCallOptions options, CancellationToken cancellationToken)
        {
            var remoteDispatcher = GetRemoteDispatcher(command);
            if (remoteDispatcher != null)
            {
                return await remoteDispatcher.DispatchAsync(command, options, cancellationToken);
            }
            else
            {
                return await _dispatcher.DispatchAsync(command, cancellationToken);
            }
        }

        #region IGrpcCommandDispatcher

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

        private IGrpcCommandDispatcher GetRemoteDispatcher<T>(T command)
        {
            var dispatcher = _clientManager.ExistsClientFor(command) ? _grpcDispatcher : null;
            return dispatcher;
        }
    }
}
