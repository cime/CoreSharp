using System.Linq;
using CoreSharp.DataAccess;
using NHibernate;

namespace CoreSharp.NHibernate.Store
{
    internal class DbStore : IDbStore
    {
        private readonly ISession _session;

        public DbStore(ISession session)
        {
            _session = session;
        }

        public IQueryable<T> Query<T>() where T : IEntity
        {
            return _session.Query<T>();
        }

        public IQueryable<T> QueryLocal<T>() where T : IEntity
        {
            return _session.Local<T>().AsQueryable();
        }

        public T Load<T>(object id) where T : IEntity
        {
            return _session.Load<T>(id);
        }

        public T Get<T>(object id) where T : IEntity
        {
            return _session.Get<T>(id);
        }

        public T Find<T>(object id) where T : class, IEntity
        {
            return QueryLocal<T>().SingleOrDefault(x => x.GetId() == id) ?? Get<T>(id);
        }

        public void Save(IEntity model)
        {
            _session.SaveOrUpdate(model);
        }

        public void Delete(IEntity model)
        {
            _session.Delete(model);
        }

        public void Refresh(IEntity model)
        {
            _session.Refresh(model);
        }

        public object Unproxy(object maybeProxy)
        {
            return _session.GetSessionImplementation().PersistenceContext.Unproxy(maybeProxy);
        }
    }

    internal class DbStore<T> : IDbStore<T>
        where T : class, IEntity
    {
        private readonly ISession _session;

        public DbStore(ISession session)
        {
            _session = session;
        }

        public IQueryable<T> Query()
        {
            return _session.Query<T>();
        }

        public IQueryable<T> QueryLocal()
        {
            return _session.Local<T>().AsQueryable();
        }

        public T Load(object id)
        {
            return _session.Load<T>(id);
        }

        public T Get(object id)
        {
            return _session.Get<T>(id);
        }

        public T Find(object id)
        {
            return QueryLocal().SingleOrDefault(x => x.GetId() == id) ?? Get(id);
        }

        public void Save(IEntity model)
        {
            _session.SaveOrUpdate(model);
        }

        public void Delete(IEntity model)
        {
            _session.Delete(model);
        }

        public void Refresh(IEntity model)
        {
            _session.Refresh(model);
        }

        public T Unproxy(object maybeProxy)
        {
            return (T)_session.GetSessionImplementation().PersistenceContext.Unproxy(maybeProxy);
        }
    }
}
