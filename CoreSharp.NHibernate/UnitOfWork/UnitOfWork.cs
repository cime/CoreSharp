using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.UnitOfWork;
using NHibernate;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CoreSharp.NHibernate.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbStore _dbStore;
        private readonly ITransaction _transaction;
        private readonly Scope _scope;
        private readonly ISession _session;

        public UnitOfWork(Container container, IsolationLevel isolationLevel)
        {
            _scope = AsyncScopedLifestyle.BeginScope(container);
            _dbStore = _scope.GetInstance<IDbStore>();
            _session = GetSession(_dbStore);
            _transaction = _session.BeginTransaction(isolationLevel);
        }

        public IQueryable<T> Query<T>() where T : IEntity
        {
            return _dbStore.Query<T>();
        }

        public IQueryable<T> QueryLocal<T>() where T : IEntity
        {
            return _dbStore.QueryLocal<T>();
        }

        public T Load<T>(object id) where T : IEntity
        {
            return _dbStore.Load<T>(id);
        }

        public T Get<T>(object id) where T : IEntity
        {
            return _dbStore.Get<T>(id);
        }

        public T Find<T>(object id) where T : class, IEntity
        {
            return _dbStore.Find<T>(id);
        }

        public void Save(IEntity model)
        {
            _dbStore.Save(model);
        }

        public void Delete(IEntity model)
        {
            _dbStore.Delete(model);
        }

        public void Flush()
        {
            _dbStore.Flush();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        private static ISession GetSession(IDbStore dbStore)
        {
            return dbStore.GetMemberValue("_session") as ISession;
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _scope.Dispose();
        }
    }
}
