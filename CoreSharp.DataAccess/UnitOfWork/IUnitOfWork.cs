using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDbStore, IDisposable
    {
        void Commit();
        void Rollback();
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
