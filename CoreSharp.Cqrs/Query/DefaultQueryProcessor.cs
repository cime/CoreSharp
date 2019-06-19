using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;

namespace CoreSharp.Common.Query
{
    internal class DefaultQueryProcessor : IQueryProcessor
    {
        private readonly Container _container;

        public DefaultQueryProcessor(Container container)
        {
            _container = container;
        }

        public TResult Process<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = (IQueryHandler<IQuery<TResult>, TResult>)_container.GetInstance(handlerType);

            return handler.Handle(query);
        }

        public Task<TResult> ProcessAsync<TResult>(IAsyncQuery<TResult> query)
        {
            var handlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = (IAsyncQueryHandler <IAsyncQuery<TResult>, TResult>)_container.GetInstance(handlerType);

            return handler.HandleAsync((dynamic)query, CancellationToken.None);
        }
    }
}
