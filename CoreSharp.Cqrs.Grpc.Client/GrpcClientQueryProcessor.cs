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

        public TResult Handle<TResult>(IQuery<TResult> query)
        {
            var rsp = GetClientForQuery(query).Execute<IQuery<TResult>, TResult>(query, default).Result;
            return rsp.Value;
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query)
        {
            var rsp = await GetClientForQuery(query).Execute<IAsyncQuery<TResult>, TResult>(query, default);
            return rsp.Value;
        }

        public async Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, CancellationToken cancellationToken)
        {
            var rsp = await GetClientForQuery(query).Execute<IAsyncQuery<TResult>, TResult>(query, cancellationToken);
            return rsp.Value;
        }

        private GrpcCqrsClient GetClientForQuery(object query)
        {
            var assemlby = query.GetType().Assembly;
            return _clients.First(x => x.ContractsAssemblies.Contains(assemlby));
        }

    }
}
