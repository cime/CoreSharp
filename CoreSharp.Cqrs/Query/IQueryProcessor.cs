using System.Threading.Tasks;

namespace CoreSharp.Cqrs.Query
{
    public interface IQueryProcessor
    {
        TResult Handle<TResult>(IQuery<TResult> query);

        Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query);
    }
}
