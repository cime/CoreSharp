#nullable disable

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Decorators;
using NHibernate.Intercept;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Type;

// ReSharper disable once CheckNamespace
namespace NHibernate
{
    public static class SessionExtensions
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<T> DeepCopy<T>(this ISession session, IEnumerable<T> entities) where T : class
        {
            var resolvedEntities = new Dictionary<object, object>();

            return entities.Select(entity => session.DeepCopy(entity, resolvedEntities)).ToList();
        }

        public static T DeepCopy<T>(this ISession session, T entity) where T : class
        {
            // forward to resolver
            return (T)session.DeepCopy(entity, entity.GetType(), new Dictionary<object, object>());
        }

        public static IEnumerable DeepCopy(this ISession session, IEnumerable entities)
        {
            var collection = (IEnumerable)CreateNewCollection(entities.GetType());
            var resolvedEntities = new Dictionary<object, object>();

            foreach (var entity in entities)
            {
                AddItemToCollection(collection, session.DeepCopy(entity, entity.GetType(), resolvedEntities));
            }

            return collection;
        }

        private static T DeepCopy<T>(this ISession session, T entity, IDictionary<object, object> resolvedEntities) where T : class
        {
            return (T)session.DeepCopy(entity, session.GetProxyRealType(entity), resolvedEntities);
        }

        private static object DeepCopy(this ISession session, object entity, System.Type entityType, IDictionary<object, object> resolvedEntities)
        {
            if (entity == null)
            {
                return entityType.GetDefaultValue();
            }

            if (!NHibernateUtil.IsInitialized(entity))
            {
                return entityType.GetDefaultValue();
            }

            if (resolvedEntities.ContainsKey(entity))
            {
                return resolvedEntities[entity];
            }

            var copiedEntity = Activator.CreateInstance(entityType);

            resolvedEntities.Add(entity, copiedEntity);

            IClassMetadata entityMetadata;

            try
            {
                entityMetadata = session.SessionFactory.GetClassMetadata(entityType);
            }
            catch (Exception)
            {
                return entityType.GetDefaultValue();
            }

            var propertyInfos = entityType.GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                IType entityPropertyType;
                try
                {
                    entityPropertyType = entityMetadata.GetPropertyType(propertyInfo.Name);
                }
                catch (Exception)
                {
                    continue;
                }

                if (!NHibernateUtil.IsPropertyInitialized(entity, propertyInfo.Name))
                    continue;

                var propertyValue = propertyInfo.GetValue(entity, null);

                if (!NHibernateUtil.IsInitialized(propertyValue))
                {
                    continue;
                }

                var propType = propertyInfo.PropertyType;

                if (entityPropertyType.IsCollectionType)
                {
                    var propertyList = CreateNewCollection(propType);
                    propertyInfo.SetValue(copiedEntity, propertyList, null);
                    AddItemToCollection(propertyList, propertyValue, o => session.DeepCopy(o, session.GetProxyRealType(o), resolvedEntities));
                }
                else if (entityPropertyType.IsEntityType)
                {
                    propertyInfo.SetValue(copiedEntity, session.DeepCopy(propertyValue, propType, resolvedEntities), null);
                }
                else if (propType.IsPrimitive || propType.IsValueType || propType.IsEnum || propType == typeof(string))
                {
                    propertyInfo.SetValue(copiedEntity, propertyValue, null);
                }
            }

            return copiedEntity;
        }

        /// <summary>
        /// Gets the underlying class type of a persistent object that may be proxied
        /// </summary>
        public static System.Type GetProxyRealType(this ISession session, object proxy)
        {
            var obj = proxy;

            if (proxy.IsProxy())
            {
                obj = ((INHibernateProxy) proxy).HibernateLazyInitializer.GetImplementation();
            }

            var fieldAccessor = FieldInterceptionHelper.ExtractFieldInterceptor(obj);

            if (fieldAccessor != null)
            {
                return fieldAccessor.MappedClass;
            }

            return obj.GetType();
        }

        //can be an interface
        private static object CreateNewCollection(System.Type collectionType)
        {
            var concreteCollType = GetCollectionImplementation(collectionType);

            if (collectionType.IsGenericType)
            {
                concreteCollType = concreteCollType.MakeGenericType(collectionType.GetGenericArguments()[0]);
            }

            return Activator.CreateInstance(concreteCollType);
        }

        private static void AddItemToCollection(object collection, object item, Func<object, object> editBeforeAdding = null)
        {
            var addMethod = collection.GetType().GetInterfaces()
                        .SelectMany(o => o.GetMethods())
                        .First(o => o.Name == "Add");

            var itemColl = item as IEnumerable;

            if (itemColl != null)
            {
                foreach (var colItem in itemColl)
                {
                    addMethod.Invoke(collection,
                                     editBeforeAdding != null
                                     ? new[] { editBeforeAdding(colItem) }
                                     : new[] { colItem });
                }
            }
            else
            {
                addMethod.Invoke(collection,
                                     editBeforeAdding != null
                                     ? new[] { editBeforeAdding(item) }
                                     : new[] { item });
            }
        }

        private static System.Type GetCollectionImplementation(System.Type collectionType)
        {
            if (collectionType.IsAssignableToGenericType(typeof(ISet<>)))
            {
                return typeof(HashSet<>);
            }

            if (collectionType.IsAssignableToGenericType(typeof(IList<>)))
            {
                return typeof(List<>);
            }

            if (collectionType.IsAssignableToGenericType(typeof(ICollection<>)))
            {
                return typeof(List<>);
            }

            if (collectionType.IsAssignableToGenericType(typeof(IEnumerable<>)))
            {
                return typeof(HashSet<>);
            }

            throw new NotSupportedException(collectionType.FullName);
        }

        public static bool IsDirtyEntity(this ISession session, object entity)
        {
            var sessionImpl = session.GetSessionImplementation();
            var persistenceContext = sessionImpl.PersistenceContext;
            var oldEntry = persistenceContext.GetEntry(entity);

            if ((oldEntry == null) && (entity is INHibernateProxy))
            {
                var proxy = entity as INHibernateProxy;
                var obj = sessionImpl.PersistenceContext.Unproxy(proxy);
                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
            }

            var className = oldEntry.EntityName;
            var persister = sessionImpl.Factory.GetEntityPersister(className);
            var oldState = oldEntry.LoadedState;
            var currentState = persister.GetPropertyValues(entity);

            var dirtyProps = oldState.Select((o, i) => (oldState[i] == currentState[i]) ? -1 : i).Where(x => x >= 0).ToArray();

            return dirtyProps.Length > 0;
        }

        public static bool IsDirtyProperty(this ISession session, object entity, string propertyName)
        {
            var sessionImpl = session.GetSessionImplementation();
            var persistenceContext = sessionImpl.PersistenceContext;
            var oldEntry = persistenceContext.GetEntry(entity);

            if ((oldEntry == null) && (entity is INHibernateProxy))
            {
                var proxy = entity as INHibernateProxy;
                var obj = sessionImpl.PersistenceContext.Unproxy(proxy);

                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
            }

            var className = oldEntry.EntityName;
            var persister = sessionImpl.Factory.GetEntityPersister(className);
            var oldState = oldEntry.LoadedState;
            var currentState = persister.GetPropertyValues(entity);
            var dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);
            var index = Array.IndexOf(persister.PropertyNames, propertyName);

            var isDirty = (dirtyProps != null) && (Array.IndexOf(dirtyProps, index) != -1);

            return isDirty;
        }

        public static object GetOriginalEntityProperty(this ISession session, object entity, string propertyName)
        {
            var sessionImpl = session.GetSessionImplementation();
            var persistenceContext = sessionImpl.PersistenceContext;
            var oldEntry = persistenceContext.GetEntry(entity);

            if ((oldEntry == null) && (entity is INHibernateProxy))
            {
                var proxy = entity as INHibernateProxy;
                var obj = sessionImpl.PersistenceContext.Unproxy(proxy);
                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
            }

            var className = oldEntry.EntityName;
            var persister = sessionImpl.Factory.GetEntityPersister(className);
            var oldState = oldEntry.LoadedState;
            var currentState = persister.GetPropertyValues(entity);
            var dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);
            var index = Array.IndexOf(persister.PropertyNames, propertyName);
            var isDirty = (dirtyProps != null) && (Array.IndexOf(dirtyProps, index) != -1);

            return isDirty ? oldState[index] : currentState[index];
        }

        private static readonly ConcurrentDictionary<System.Type, string> TableNamesCache = new  ConcurrentDictionary<System.Type, string>();

        public static string GetTableName<T>(this ISession session)
            where T: class
        {
            var type = typeof (T);

            return TableNamesCache.GetOrAdd(type, t => ((AbstractEntityPersister)session.SessionFactory.GetClassMetadata(t)).TableName);
        }

        public static T GetRandom<T>(this ISession session)
            where T : class
        {
            var impl = session.GetSessionImplementation();
            var pc = impl.PersistenceContext;

            var entities = new List<T>();

            foreach (var key in pc.EntityEntries.Keys)
            {
                if (key is T)
                {
                    entities.Add((T)key);
                }
            }

            if (entities.Count == 0)
            {
                entities = session.CreateCriteria(typeof(T)).List<T>().ToList();
            }

            return entities.Count == 0 ? null : entities[Random.Next(0, entities.Count)];
        }

        public static void RollbackTransaction(this ISession session)
        {
            if (session.Transaction.WasRolledBack || !session.Transaction.IsActive)
                return;
            session.Transaction.Rollback();
            session.Transaction.Dispose();
        }

        public static void RollbackTransaction(this IStatelessSession session)
        {
            if (session.Transaction.WasRolledBack || !session.Transaction.IsActive)
                return;
            session.Transaction.Rollback();
            session.Transaction.Dispose();
        }

        public static bool CommitTransaction(this ISession session)
        {
            if (session.Transaction.WasRolledBack || !session.Transaction.IsActive)
                return false;
            session.Transaction.Commit();
            return true;
        }

        public static bool CommitTransaction(this IStatelessSession session)
        {
            if (session.Transaction.WasRolledBack || !session.Transaction.IsActive)
                return false;
            session.Transaction.Commit();
            return true;
        }

        public static ISession UnWrap(this ISession session)
        {
            var decorator = session as SessionDecorator;

            return decorator != null ? decorator.Session : session;
        }

        public static IEnumerable<T> Local<T>(this ISession session)
        {
            var impl = session.GetSessionImplementation();
            var pc = impl.PersistenceContext;

            foreach (var key in pc.EntityEntries.Keys)
            {
                if (key is T)
                {
                    yield return ((T)key);
                }
            }
        }

        public static T LoadIf<T>(this ISession session, object id)
            where T : IEntity
        {
            if (id is string str && string.IsNullOrWhiteSpace(str))
            {
                return default(T);
            }

            return id == null ? default(T) : session.Load<T>(id);
        }

        public static T GetIf<T>(this ISession session, object id)
            where T : IEntity
        {
            if (id is string str && string.IsNullOrWhiteSpace(str))
            {
                return default(T);
            }

            return id == null ? default(T) : session.Get<T>(id);
        }
    }
}
