using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using CoreSharp.NHibernate.Events;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NHibernate.Type;

namespace CoreSharp.NHibernate.Decorators
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SessionDecorator : ISession
    {
        private readonly IEventPublisher _eventPublisher;

        internal ISession Session { get; }

        public SessionDecorator(ISession session, IEventPublisher eventPublisher)
        {
            Session = session;
            _eventPublisher = eventPublisher;
        }

        public void Dispose()
        {
            Session.Dispose();
        }

        public ISharedSessionBuilder SessionWithOptions()
        {
            return Session.SessionWithOptions();
        }

        public FlushMode FlushMode
        {
            get => Session.FlushMode;
            set => Session.FlushMode = value;
        }

        public CacheMode CacheMode
        {
            get => Session.CacheMode;
            set => Session.CacheMode = value;
        }

        public ISessionFactory SessionFactory => Session.SessionFactory;

        public DbConnection Connection => Session.Connection;

        public bool IsOpen => Session.IsOpen;

        public bool IsConnected => Session.IsConnected;

        public bool DefaultReadOnly
        {
            get => Session.DefaultReadOnly;
            set => Session.DefaultReadOnly = value;
        }

        public ITransaction Transaction => Session.Transaction;

        public ISessionStatistics Statistics => Session.Statistics;

        public void Flush()
        {
            _eventPublisher.PublishAsync(new SessionFlushingEvent(Session)).GetAwaiter().GetResult();
            ((ISessionImplementor)Session).Flush();
            _eventPublisher.PublishAsync(new SessionFlushedEvent(Session)).GetAwaiter().GetResult();
        }

        public DbConnection Disconnect()
        {
            return Session.Disconnect();
        }

        public void Reconnect()
        {
            Session.Reconnect();
        }

        public void Reconnect(DbConnection connection)
        {
            Session.Reconnect(connection);
        }

        public DbConnection Close()
        {
            return Session.Close();
        }

        public void CancelQuery()
        {
            Session.CancelQuery();
        }

        public bool IsDirty()
        {
            return Session.IsDirty();
        }

        public bool IsReadOnly(object entityOrProxy)
        {
            return Session.IsReadOnly(entityOrProxy);
        }

        public void SetReadOnly(object entityOrProxy, bool readOnly)
        {
            Session.SetReadOnly(entityOrProxy, readOnly);
        }

        public object GetIdentifier(object obj)
        {
            return Session.GetIdentifier(obj);
        }

        public bool Contains(object obj)
        {
            return Session.Contains(obj);
        }

        public void Evict(object obj)
        {
            Session.Evict(obj);
        }

        public object Load(Type theType, object id, LockMode lockMode)
        {
            return Session.Load(theType, id, lockMode);
        }

        public object Load(string entityName, object id, LockMode lockMode)
        {
            return Session.Load(entityName, id, lockMode);
        }

        public object Load(Type theType, object id)
        {
            return Session.Load(theType, id);
        }

        public T Load<T>(object id, LockMode lockMode)
        {
            return Session.Load<T>(id, lockMode);
        }

        public T Load<T>(object id)
        {
            return Session.Load<T>(id);
        }

        public object Load(string entityName, object id)
        {
            return Session.Load(entityName, id);
        }

        public void Load(object obj, object id)
        {
            Session.Load(obj, id);
        }

        public void Replicate(object obj, ReplicationMode replicationMode)
        {
            Session.Replicate(obj, replicationMode);
        }

        public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
        {
            Session.Replicate(entityName, obj, replicationMode);
        }

        public object Save(object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            return Session.Save(obj);
        }

        public void Save(object obj, object id)
        {
            Session.Save(obj, id);
        }

        public object Save(string entityName, object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            return Session.Save(entityName, obj);
        }

        public void Save(string entityName, object obj, object id)
        {
            Session.Save(entityName, obj, id);
        }

        public void SaveOrUpdate(object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.SaveOrUpdate(obj);
        }

        public void SaveOrUpdate(string entityName, object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.SaveOrUpdate(entityName, obj);
        }

        public void SaveOrUpdate(string entityName, object obj, object id)
        {
            Session.SaveOrUpdate(entityName, obj, id);
        }

        public void Update(object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.Update(obj);
        }

        public void Update(object obj, object id)
        {
            Session.Update(obj, id);
        }

        public void Update(string entityName, object obj)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.Update(entityName, obj);
        }

        public void Update(string entityName, object obj, object id)
        {
            Session.Update(entityName, obj, id);
        }

        public object Merge(object obj)
        {
            return Session.Merge(obj);
        }

        public object Merge(string entityName, object obj)
        {
            return Session.Merge(entityName, obj);
        }

        public T Merge<T>(T entity) where T : class
        {
            return Session.Merge(entity);
        }

        public T Merge<T>(string entityName, T entity) where T : class
        {
            return Session.Merge(entityName, entity);
        }

        public void Persist(object obj)
        {
            Session.Persist(obj);
        }

        public void Persist(string entityName, object obj)
        {
            Session.Persist(entityName, obj);
        }

        public void Delete(object obj)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.Delete(obj);
        }

        public void Delete(string entityName, object obj)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();

            Session.Delete(obj);
        }

        public int Delete(string query)
        {
            return Session.Delete(query);
        }

        public int Delete(string query, object value, IType type)
        {
            return Session.Delete(query, value, type);
        }

        public int Delete(string query, object[] values, IType[] types)
        {
            return Session.Delete(query, values, types);
        }

        public void Lock(object obj, LockMode lockMode)
        {
            Session.Lock(obj, lockMode);
        }

        public void Lock(string entityName, object obj, LockMode lockMode)
        {
            Session.Lock(entityName, obj, lockMode);
        }

        public void Refresh(object obj)
        {
            Session.Refresh(obj);
        }

        public void Refresh(object obj, LockMode lockMode)
        {
            Session.Refresh(obj, lockMode);
        }

        public LockMode GetCurrentLockMode(object obj)
        {
            return Session.GetCurrentLockMode(obj);
        }

        public ITransaction BeginTransaction()
        {
            return Session.BeginTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Session.BeginTransaction(isolationLevel);
        }

        public void JoinTransaction()
        {
            Session.JoinTransaction();
        }

        public ICriteria CreateCriteria<T>() where T : class
        {
            return Session.CreateCriteria<T>();
        }

        public ICriteria CreateCriteria<T>(string alias) where T : class
        {
            return Session.CreateCriteria<T>(alias);
        }

        public ICriteria CreateCriteria(Type persistentClass)
        {
            return Session.CreateCriteria(persistentClass);
        }

        public ICriteria CreateCriteria(Type persistentClass, string alias)
        {
            return Session.CreateCriteria(persistentClass, alias);
        }

        public ICriteria CreateCriteria(string entityName)
        {
            return Session.CreateCriteria(entityName);
        }

        public ICriteria CreateCriteria(string entityName, string alias)
        {
            return Session.CreateCriteria(entityName, alias);
        }

        public IQueryOver<T, T> QueryOver<T>() where T : class
        {
            return Session.QueryOver<T>();
        }

        public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
        {
            return Session.QueryOver(alias);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName) where T : class
        {
            return Session.QueryOver<T>(entityName);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class
        {
            return Session.QueryOver(entityName, alias);
        }

        public IQuery CreateQuery(string queryString)
        {
            return Session.CreateQuery(queryString);
        }

        public IQuery CreateFilter(object collection, string queryString)
        {
            return Session.CreateFilter(collection, queryString);
        }

        public IQuery GetNamedQuery(string queryName)
        {
            return Session.GetNamedQuery(queryName);
        }

        public ISQLQuery CreateSQLQuery(string queryString)
        {
            return Session.CreateSQLQuery(queryString);
        }

        public void Clear()
        {
            Session.Clear();
        }

        public object Get(Type clazz, object id)
        {
            return Session.Get(clazz, id);
        }

        public object Get(Type clazz, object id, LockMode lockMode)
        {
            return Session.Get(clazz, id, lockMode);
        }

        public object Get(string entityName, object id)
        {
            return Session.Get(entityName, id);
        }

        public T Get<T>(object id)
        {
            return Session.Get<T>(id);
        }

        public T Get<T>(object id, LockMode lockMode)
        {
            return Session.Get<T>(id, lockMode);
        }

        public string GetEntityName(object obj)
        {
            return Session.GetEntityName(obj);
        }

        public IFilter EnableFilter(string filterName)
        {
            return Session.EnableFilter(filterName);
        }

        public IFilter GetEnabledFilter(string filterName)
        {
            return Session.GetEnabledFilter(filterName);
        }

        public void DisableFilter(string filterName)
        {
            Session.DisableFilter(filterName);
        }

        [Obsolete]
        public IMultiQuery CreateMultiQuery()
        {
            return Session.CreateMultiQuery();
        }

        public ISession SetBatchSize(int batchSize)
        {
            return Session.SetBatchSize(batchSize);
        }

        public ISessionImplementor GetSessionImplementation()
        {
            return Session.GetSessionImplementation();
        }

        [Obsolete]
        public IMultiCriteria CreateMultiCriteria()
        {
            return Session.CreateMultiCriteria();
        }

        [Obsolete]
        public ISession GetSession(EntityMode entityMode)
        {
            return Session.GetSession(entityMode);
        }

        public IQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }

        public IQueryable<T> Query<T>(string entityName)
        {
            return Session.Query<T>(entityName);
        }




        #region Async

        public async Task FlushAsync(CancellationToken cancellationToken)
        {

            await _eventPublisher.PublishAsync(new SessionFlushingEvent(Session), cancellationToken);
            await ((ISessionImplementor)Session).FlushAsync(cancellationToken);
            await _eventPublisher.PublishAsync(new SessionFlushedEvent(Session), cancellationToken);
        }

        public Task<bool> IsDirtyAsync(CancellationToken cancellationToken)
        {
            return Session.IsDirtyAsync(cancellationToken);
        }

        public Task EvictAsync(object obj, CancellationToken cancellationToken)
        {
            return Session.EvictAsync(obj, cancellationToken);
        }

        public Task<object> LoadAsync(Type theType, object id, LockMode lockMode,
            CancellationToken cancellationToken)
        {
            return Session.LoadAsync(theType, id, lockMode, cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, LockMode lockMode,
            CancellationToken cancellationToken)
        {
            return Session.LoadAsync(entityName, id, lockMode, cancellationToken);
        }

        public Task<object> LoadAsync(Type theType, object id, CancellationToken cancellationToken)
        {
            return Session.LoadAsync(theType, id, cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken)
        {
            return Session.LoadAsync<T>(id, lockMode, cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken)
        {
            return Session.LoadAsync<T>(id, cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken)
        {
            return Session.LoadAsync(entityName, id, cancellationToken);
        }

        public Task LoadAsync(object obj, object id, CancellationToken cancellationToken)
        {
            return Session.LoadAsync(obj, id, cancellationToken);
        }

        public Task ReplicateAsync(object obj, ReplicationMode replicationMode,
            CancellationToken cancellationToken)
        {
            return Session.ReplicateAsync(obj, replicationMode, cancellationToken);
        }

        public Task ReplicateAsync(string entityName, object obj, ReplicationMode replicationMode,
            CancellationToken cancellationToken)
        {
            return Session.ReplicateAsync(entityName, obj, replicationMode, cancellationToken);
        }

        public async Task<object> SaveAsync(object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            return await Session.SaveAsync(obj, cancellationToken);
        }

        public Task SaveAsync(object obj, object id, CancellationToken cancellationToken)
        {
            return Session.SaveAsync(obj, id, cancellationToken);
        }

        public async Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            return await Session.SaveAsync(entityName, obj, cancellationToken);
        }

        public Task SaveAsync(string entityName, object obj, object id, CancellationToken cancellationToken)
        {
            return Session.SaveAsync(entityName, obj, id, cancellationToken);
        }

        public async Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.SaveOrUpdateAsync(obj, cancellationToken);
        }

        public async Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.SaveOrUpdateAsync(entityName, obj, cancellationToken);
        }

        public Task SaveOrUpdateAsync(string entityName, object obj, object id,
            CancellationToken cancellationToken)
        {
            return Session.SaveOrUpdateAsync(entityName, obj, id, cancellationToken);
        }

        public async Task UpdateAsync(object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.UpdateAsync(obj, cancellationToken);
        }

        public Task UpdateAsync(object obj, object id, CancellationToken cancellationToken)
        {
            return Session.UpdateAsync(obj, id, cancellationToken);
        }

        public async Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.UpdateAsync(entityName, obj, cancellationToken);
        }

        public Task UpdateAsync(string entityName, object obj, object id,
            CancellationToken cancellationToken)
        {
            return Session.UpdateAsync(entityName, obj, id, cancellationToken);
        }

        public Task<object> MergeAsync(object obj, CancellationToken cancellationToken)
        {
            return Session.MergeAsync(obj, cancellationToken);
        }

        public Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            return Session.MergeAsync(entityName, obj, cancellationToken);
        }

        public Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            return Session.MergeAsync(entity, cancellationToken);
        }

        public Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken) where T : class
        {
            return Session.MergeAsync(entityName, entity, cancellationToken);
        }

        public Task PersistAsync(object obj, CancellationToken cancellationToken)
        {
            return Session.PersistAsync(obj, cancellationToken);
        }

        public Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            return Session.PersistAsync(entityName, obj, cancellationToken);
        }

        public async Task DeleteAsync(object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.DeleteAsync(obj, cancellationToken);
        }

        public async Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken)
        {
            var eventType = typeof(EntityDeletingEvent<>).MakeGenericType(obj.GetType());
            var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, obj, this);
            await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);

            await Session.DeleteAsync(entityName, obj, cancellationToken);
        }

        public Task<int> DeleteAsync(string query, CancellationToken cancellationToken)
        {
            return Session.DeleteAsync(query, cancellationToken);
        }

        public Task<int> DeleteAsync(string query, object value, IType type, CancellationToken cancellationToken)
        {
            return Session.DeleteAsync(query, value, type, cancellationToken);
        }

        public Task<int> DeleteAsync(string query, object[] values, IType[] types,
            CancellationToken cancellationToken)
        {
            return Session.DeleteAsync(query, values, types, cancellationToken);
        }

        public Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken)
        {
            return Session.LockAsync(obj, lockMode, cancellationToken);
        }

        public Task LockAsync(string entityName, object obj, LockMode lockMode,
            CancellationToken cancellationToken)
        {
            return Session.LockAsync(entityName, obj, lockMode, cancellationToken);
        }

        public Task RefreshAsync(object obj, CancellationToken cancellationToken)
        {
            return Session.RefreshAsync(obj, cancellationToken);
        }

        public Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken)
        {
            return Session.RefreshAsync(obj, lockMode, cancellationToken);
        }

        public Task<IQuery> CreateFilterAsync(object collection, string queryString,
            CancellationToken cancellationToken)
        {
            return Session.CreateFilterAsync(collection, queryString, cancellationToken);
        }

        public Task<object> GetAsync(Type clazz, object id, CancellationToken cancellationToken)
        {
            return Session.GetAsync(clazz, id, cancellationToken);
        }

        public Task<object> GetAsync(Type clazz, object id, LockMode lockMode, CancellationToken cancellationToken)
        {
            return Session.GetAsync(clazz, id, lockMode, cancellationToken);
        }

        public Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken)
        {
            return Session.GetAsync(entityName, id, cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, CancellationToken cancellationToken)
        {
            return Session.GetAsync<T>(id, cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken)
        {
            return Session.GetAsync<T>(id, lockMode, cancellationToken);
        }

        public Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken)
        {
            return Session.GetEntityNameAsync(obj, cancellationToken);
        }

        #endregion
    }
}
