using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;

namespace CoreSharp.Cqrs.Query
{
    internal class DefaultQueryProcessor : IQueryProcessor
    {
        private readonly Container _container;

        public DefaultQueryProcessor(Container container)
        {
            _container = container;
        }

        public TResult Handle<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _container.GetInstance(handlerType);

            var result = (TResult) handler.Handle((dynamic) query);

            return result;
        }

        public Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query, CancellationToken cancellationToken)
        {
            var handlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _container.GetInstance(handlerType);

            var result = (Task<TResult>) handler.HandleAsync((dynamic) query, (dynamic) cancellationToken);

            return result;
        }

        public Task<TResult> HandleAsync<TResult>(IAsyncQuery<TResult> query)
        {
            return HandleAsync(query, CancellationToken.None);
        }
    }
}
