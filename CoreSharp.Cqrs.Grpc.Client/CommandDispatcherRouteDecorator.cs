using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;

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

        public void Dispatch(ICommand command)
        {
            GetDispatcher(command).Dispatch(command);
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command)
        {
            return GetDispatcher(command).Dispatch(command);
        }

        public async Task DispatchAsync(IAsyncCommand command)
        {
            await GetDispatcher(command).DispatchAsync(command);
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command)
        {
            return await GetDispatcher(command).DispatchAsync(command);
        }

        public async Task DispatchAsync(IAsyncCommand command, CancellationToken cancellationToken)
        {
            await GetDispatcher(command).DispatchAsync(command, cancellationToken);
        }

        public async Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, CancellationToken cancellationToken)
        {
            return await GetDispatcher(command).DispatchAsync(command, cancellationToken);
        }

        private ICommandDispatcher GetDispatcher<T>(T command)
        {
            var dispatcher = _clientManager.ExistsClientFor(command) ? _grpcDispatcher : _dispatcher;
            return dispatcher;
        }
    }
}
