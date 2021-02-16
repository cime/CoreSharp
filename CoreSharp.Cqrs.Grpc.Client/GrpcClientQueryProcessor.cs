using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class GrpcClientQueryProcessor : IGrpcQueryProcessor
    {
        private readonly IEnumerable<GrpcCqrsClient> _clients;

        public GrpcClientQueryProcessor(IEnumerable<GrpcCqrsClient> clients)
        {
            _clients = clients;
        }

        public TResult Handle<TResult>(IQuery<TResult> query, GrpcCqrsCallOptions callOptions)
        {
            var rsp = GetClientForQuery(query).Execute<IQuery<TResult>, TResult>(query, callOptions, default).Result;
            return rsp.Value;
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, GrpcCqrsCallOptions callOptions, CancellationToken cancellationToken)
        {
            var rsp = await GetClientForQuery(query).Execute<IAsyncQuery<TResult>, TResult>(query, callOptions, cancellationToken);
            return rsp.Value;
        }

        #region IQueryProcessor

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

        private GrpcCqrsClient GetClientForQuery(object query)
        {
            var assemlby = query.GetType().Assembly;
            return _clients.First(x => x.ContractsAssemblies.Contains(assemlby));
        }
    }
}
