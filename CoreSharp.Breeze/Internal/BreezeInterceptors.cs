using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Breeze.Events;
using CoreSharp.Common.Events;
using CoreSharp.DataAccess;
using SimpleInjector;

namespace CoreSharp.Breeze.Internal
{
    internal class BreezeInterceptors : IAsyncEventHandler<BreezeAfterFlushAsyncEvent>, IAsyncEventHandler<BreezeBeforeSaveAsyncEvent>
    {
        private readonly Container _container;

        public BreezeInterceptors(Container container)
        {
            _container = container;
        }

        public async Task HandleAsync(BreezeAfterFlushAsyncEvent @event, CancellationToken cancellationToken)
        {
            var entityGroups = @event.EntitiesToPersist.GroupBy(x => ((IEntity)x.Entity).GetType());

            foreach (var kv in entityGroups)
            {
                var interceptorType = typeof(IBreezeAfterSaveInterceptor<>).MakeGenericType(kv.Key);
                var  interceptor = ((IServiceProvider)_container).GetService(interceptorType) as IBreezeAfterSaveInterceptor;

                if (interceptor != null)
                {
                    await interceptor.AfterSaveAsync(kv.ToList(), cancellationToken);
                }
            }
        }

        public async Task HandleAsync(BreezeBeforeSaveAsyncEvent @event, CancellationToken cancellationToken)
        {
            var entityGroups = @event.EntitiesToPersist.GroupBy(x => ((IEntity)x.Entity).GetType());

            foreach (var kv in entityGroups)
            {
                var interceptorType = typeof(IBreezeBeforeSaveInterceptor<>).MakeGenericType(kv.Key);
                var interceptor = ((IServiceProvider)_container).GetService(interceptorType) as IBreezeBeforeSaveInterceptor;

                if (interceptor != null)
                {
                    await interceptor.BeforeSaveAsync(kv.ToList(), cancellationToken);
                }
            }
        }
    }
}
