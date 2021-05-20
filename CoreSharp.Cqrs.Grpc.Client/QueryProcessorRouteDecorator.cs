using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class QueryProcessorRouteDecorator : IGrpcQueryProcessor
    {

        private readonly IGrpcClientManager _clientManager;
        private readonly IQueryProcessor _processor;
        private readonly IGrpcQueryProcessor _grpcProcessor;

        public QueryProcessorRouteDecorator(IGrpcClientManager clientManager, IQueryProcessor processor, IGrpcQueryProcessor grpcProcessor)
        {
            _clientManager = clientManager;
            _processor = processor;
            _grpcProcessor = grpcProcessor;
        }

        public TResult Handle<TResult>(IQuery<TResult> query, GrpcCqrsCallOptions callOptions)
        {
            var remoteProcessor = GetRemoteProcessor(query);
            if (remoteProcessor != null)
            {
                return remoteProcessor.Handle(query, callOptions);
            }
            else
            {
                return _processor.Handle(query);
            }
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, GrpcCqrsCallOptions callOptions, CancellationToken cancellationToken)
        {
            var remoteProcessor = GetRemoteProcessor(query);
            if (remoteProcessor != null)
            {
                return await remoteProcessor.HandleAsync(query, callOptions, cancellationToken);
            }
            else
            {
                return await _processor.HandleAsync(query, cancellationToken);
            }
        }

        #region IGrpcQueryProcessor

        public TResult Handle<TResult>(IQuery<TResult> query)
        {
            return Handle(query, null);
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query)
        {
            return await HandleAsync(query, null, default);
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, CancellationToken cancellationToken)
        {
            return await HandleAsync(query, null, cancellationToken);
        }

        #endregion

        private IGrpcQueryProcessor GetRemoteProcessor<T>(T query)
        {
            return _clientManager.ExistsClientFor(query) ? _grpcProcessor : null;
        }
    }
}
