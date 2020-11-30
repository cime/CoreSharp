using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Processors
{
    public interface IGrpcCqrsServerProcessor
    {
        Task<GrpcResponseEnvelope<TResponse>> ProcessRequestAsync<TRequest, TResponse>(TRequest request, CqrsInfo info, CancellationToken cancellationToken);
    }
}
