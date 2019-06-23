using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Stat;

namespace CoreSharp.NHibernate.Decorators
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SessionFactoryDecorator : ISessionFactory
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly IEventPublisher _eventPublisher;

        public SessionFactoryDecorator(ISessionFactory sessionFactory, IEventPublisher eventPublisher)
        {
            _sessionFactory = sessionFactory;
            _eventPublisher = eventPublisher;
        }

        public void Dispose()
        {
            _sessionFactory.Dispose();
        }

        public ISessionBuilder WithOptions()
        {
            return _sessionFactory.WithOptions();
        }

        [Obsolete]
        public ISession OpenSession(DbConnection conn)
        {
            return _sessionFactory.OpenSession(conn);
        }

        [Obsolete]
        public ISession OpenSession(IInterceptor sessionLocalInterceptor)
        {
            return _sessionFactory.OpenSession(sessionLocalInterceptor);
        }

        [Obsolete]
        public ISession OpenSession(DbConnection conn, IInterceptor sessionLocalInterceptor)
        {
            return _sessionFactory.OpenSession(conn, sessionLocalInterceptor);
        }

        public ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        public IStatelessSessionBuilder WithStatelessOptions()
        {
            return _sessionFactory.WithStatelessOptions();
        }

        public IClassMetadata GetClassMetadata(Type persistentClass)
        {
            return _sessionFactory.GetClassMetadata(persistentClass);
        }

        public IClassMetadata GetClassMetadata(string entityName)
        {
            return _sessionFactory.GetClassMetadata(entityName);
        }

        public ICollectionMetadata GetCollectionMetadata(string roleName)
        {
            return _sessionFactory.GetCollectionMetadata(roleName);
        }

        public IDictionary<string, IClassMetadata> GetAllClassMetadata()
        {
            return _sessionFactory.GetAllClassMetadata();
        }

        public IDictionary<string, ICollectionMetadata> GetAllCollectionMetadata()
        {
            return _sessionFactory.GetAllCollectionMetadata();
        }

        public void Close()
        {
            _sessionFactory.Close();
        }

        public void Evict(Type persistentClass)
        {
            _sessionFactory.Evict(persistentClass);
        }

        public void Evict(Type persistentClass, object id)
        {
            _sessionFactory.Evict(persistentClass, id);
        }

        public void EvictEntity(string entityName)
        {
            _sessionFactory.EvictEntity(entityName);
        }

        public void EvictEntity(string entityName, object id)
        {
            _sessionFactory.EvictEntity(entityName, id);
        }

        public void EvictCollection(string roleName)
        {
            _sessionFactory.EvictCollection(roleName);
        }

        public void EvictCollection(string roleName, object id)
        {
            _sessionFactory.EvictCollection(roleName, id);
        }

        public void EvictQueries()
        {
            _sessionFactory.EvictQueries();
        }

        public void EvictQueries(string cacheRegion)
        {
            _sessionFactory.EvictQueries(cacheRegion);
        }

        public IStatelessSession OpenStatelessSession()
        {
            return _sessionFactory.OpenStatelessSession();
        }

        public IStatelessSession OpenStatelessSession(DbConnection connection)
        {
            return _sessionFactory.OpenStatelessSession(connection);
        }

        public FilterDefinition GetFilterDefinition(string filterName)
        {
            return _sessionFactory.GetFilterDefinition(filterName);
        }

        public ISession GetCurrentSession()
        {
            return _sessionFactory.GetCurrentSession();
        }

        public IStatistics Statistics => _sessionFactory.Statistics;

        public bool IsClosed => _sessionFactory.IsClosed;

        public ICollection<string> DefinedFilterNames => _sessionFactory.DefinedFilterNames;


        #region Async

        public Task CloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.CloseAsync(cancellationToken);
        }

        public Task EvictAsync(Type persistentClass, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictAsync(persistentClass, cancellationToken);
        }

        public Task EvictAsync(Type persistentClass, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictAsync(persistentClass, id, cancellationToken);
        }

        public Task EvictEntityAsync(string entityName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictEntityAsync(entityName, cancellationToken);
        }

        public Task EvictEntityAsync(string entityName, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictEntityAsync(entityName, id, cancellationToken);
        }

        public Task EvictCollectionAsync(string roleName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictCollectionAsync(roleName, cancellationToken);
        }

        public Task EvictCollectionAsync(string roleName, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictCollectionAsync(roleName, id, cancellationToken);
        }

        public Task EvictQueriesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictQueriesAsync(cancellationToken);
        }

        public Task EvictQueriesAsync(string cacheRegion, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sessionFactory.EvictQueriesAsync(cacheRegion, cancellationToken);
        }


        #endregion
    }
}
