using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Breeze
{
    public interface IBreezeAfterSaveInterceptor
    {
        Task AfterSaveAsync(List<EntityInfo> entitiesToPersist, CancellationToken cancellationToken);
    }

    public interface IBreezeAfterSaveInterceptor<TType> : IBreezeAfterSaveInterceptor
    {
    }
}
