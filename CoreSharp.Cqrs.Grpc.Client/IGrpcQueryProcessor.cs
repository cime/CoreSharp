using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public interface IGrpcQueryProcessor : IQueryProcessor
    {
        TResult Handle<TResult>(IQuery<TResult> query, GrpcCqrsCallOptions callOptions);

        Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, GrpcCqrsCallOptions callOptions, CancellationToken cancellationToken);
    }
}
