using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class QueryProcessorRouteDecorator : IQueryProcessor
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

        public TResult Handle<TResult>(IQuery<TResult> query)
        {
            return GetProcessor(query).Handle(query);
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query)
        {
            return await GetProcessor(query).HandleAsync(query);
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, CancellationToken cancellationToken)
        {
            return await GetProcessor(query).HandleAsync(query, cancellationToken);
        }

        private IQueryProcessor GetProcessor<T>(T command)
        {
            var dispatcher = _clientManager.ExistsClientFor(command) ? _grpcProcessor : _processor;
            return dispatcher;
        }
    }
}
