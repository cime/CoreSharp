using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Breeze
{
    public interface IBreezeBeforeSaveInterceptor
    {
        Task BeforeSaveAsync(List<EntityInfo> entitiesToPersist, CancellationToken cancellationToken);
    }

    public interface IBreezeBeforeSaveInterceptor<TType> : IBreezeBeforeSaveInterceptor
    {
    }
}
