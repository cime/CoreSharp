using System.Threading.Tasks;

namespace CoreSharp.Common.Query
{
    public interface IQueryProcessor
    {
        TResult Process<TResult>(IQuery<TResult> query);

        Task<TResult> ProcessAsync<TResult>(IAsyncQuery<TResult> query);
    }
}
