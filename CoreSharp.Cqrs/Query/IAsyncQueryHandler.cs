using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Query
{
    /// <summary>
    /// Async query handler for <typeparam name="TQuery" />
    /// </summary>
    /// <typeparam name="TQuery"> query</typeparam>
    /// <typeparam name="TResult">Return type of query</typeparam>
    public interface IAsyncQueryHandler<in TQuery, TResult> where TQuery : IAsyncQuery<TResult>
    {
        /// <summary>
        /// Handle <typeparam name="TQuery" /> async query
        /// </summary>
        /// <param name="query"><typeparamref name="TQuery"/> command</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns><typeparamref name="TResult"/></returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
