using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Query
{
    public interface IAsyncQueryHandler<in TQuery, TResult> where TQuery : IAsyncQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
