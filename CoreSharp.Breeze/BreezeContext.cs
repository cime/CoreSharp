using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using CoreSharp.Breeze.Events;
using CoreSharp.Breeze.Metadata;
using CoreSharp.Cqrs.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Metadata;
using NHibernate.Type;
using IsolationLevel = System.Data.IsolationLevel;

namespace CoreSharp.Breeze
{
    public class BreezeContext : ContextProvider, IDisposable
    {
        //protected Configuration configuration;
        private static readonly Dictionary<ISessionFactory, MetadataSchema> FactoryMetadata =
            new Dictionary<ISessionFactory, MetadataSchema>();

        private static readonly object MetadataLock = new object();
        private static readonly object BuildMetadataLock = new object();
        private readonly IBreezeConfigurator _breezeConfigurator;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        ///     Create a new context for the given session.
        ///     Each thread should have its own NHContext and Session.
        /// </summary>
        /// <param name="breezeConfig">Used for breeze config</param>
        /// <param name="session">Used for queries and updates</param>
        /// <param name="breezeConfigurator">Used for get configurations</param>
        /// <param name="eventPublisher">Used for publishing events</param>
        public BreezeContext(IBreezeConfig breezeConfig, ISession session, IBreezeConfigurator breezeConfigurator, IEventPublisher eventPublisher) : base(breezeConfig)
        {
            Session = session;
            _breezeConfigurator = breezeConfigurator;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        ///     Creates a new context using the session and metadata from the sourceContext
        /// </summary>
        /// <param name="sourceContext">source of the Session and metadata used by this new context.</param>
        /// <param name="eventPublisher">Used for publishing events</param>
        public BreezeContext(BreezeContext sourceContext, IEventPublisher eventPublisher) : base(sourceContext._breezeConfig)
        {
            _eventPublisher = eventPublisher;
            Session = sourceContext.Session;
            _breezeConfigurator = sourceContext._breezeConfigurator;
            _metadata = sourceContext.GetMetadata();
        }

        public ISession Session { get; }

        /// <summary>
        ///     Close the session
        /// </summary>
        public virtual void Dispose()
        {
            Close();
        }

        /// <summary>
        ///     Return a query for the given entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="cacheable">Whether to mark the query Cacheable.  Default is false.</param>
        /// <returns></returns>
        public NhQueryableInclude<T> GetQuery<T>(bool cacheable = false)
        {
            return new NhQueryableInclude<T>(Session.GetSessionImplementation(), cacheable);
        }

        /// <summary>
        ///     Return a cacheable query for the given entity, using the given cache region
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="cacheRegion">Cache region to use.</param>
        /// <returns></returns>
        public NhQueryableInclude<T> GetQuery<T>(string cacheRegion)
        {
            return new NhQueryableInclude<T>(Session.GetSessionImplementation(), cacheRegion);
        }

        public async Task<SaveResult> SaveChangesAsync(JObject saveBundle,
            TransactionSettings transactionSettings = null)
        {
            if (SaveWorkState == null || SaveWorkState.WasUsed) InitializeSaveState(saveBundle);

            transactionSettings = transactionSettings ?? _breezeConfig.GetTransactionSettings();
            try
            {
                if (transactionSettings.TransactionType == TransactionType.TransactionScope)
                {
                    var txOptions = transactionSettings.ToTransactionOptions();
                    using (var txScope = new TransactionScope(TransactionScopeOption.Required, txOptions))
                    {
                        await OpenAndSaveAsync(SaveWorkState);
                        txScope.Complete();
                    }
                }
                else if (transactionSettings.TransactionType == TransactionType.DbTransaction)
                {
                    OpenDbConnection();
                    using (var tran = BeginTransaction(transactionSettings.IsolationLevelAs))
                    {
                        try
                        {
                            await OpenAndSaveAsync(SaveWorkState);
                            await Session.Transaction.CommitAsync();
                        }
                        catch
                        {
                            Session.Transaction.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    await OpenAndSaveAsync(SaveWorkState);
                }
            }
            catch (EntityErrorsException e)
            {
                SaveWorkState.EntityErrors = e.EntityErrors;
                throw;
            }
            catch (Exception e2)
            {
                if (!HandleSaveException(e2, SaveWorkState)) throw;
            }
            finally
            {
                CloseDbConnection();
            }

            return SaveWorkState.ToSaveResult();
        }

        private async Task OpenAndSaveAsync(SaveWorkState saveWorkState)
        {
            saveWorkState.BeforeSave();
            await SaveChangesCoreAsync(saveWorkState);
            saveWorkState.AfterSave();
        }

        /// <summary>
        ///     Close the session
        /// </summary>
        public virtual void Close()
        {
            if (Session != null && Session.IsOpen)
            {
                Session.Close();
            }
        }

        /// <returns>The connection from the session.</returns>
        public override IDbConnection GetDbConnection()
        {
            return Session.Connection;
        }

        protected override void OpenDbConnection()
        {
            // already open when session is created
        }

        /// <summary>
        ///     Close the session and its associated db connection
        /// </summary>
        protected override void CloseDbConnection()
        {
            if (Session != null && Session.IsOpen)
            {
                var dbc = Session.Close();
                dbc?.Close();
            }
        }

        protected override IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var itran = Session.BeginTransaction(isolationLevel);
            var wrapper = new NHTransactionWrapper(itran, Session.Connection, isolationLevel);
            return wrapper;
        }

        public override object[] GetKeyValues(EntityInfo entityInfo)
        {
            return GetKeyValues(entityInfo.Entity);
        }

        public object[] GetKeyValues(object entity)
        {
            var entityType = Session.GetProxyRealType(entity);
            var classMeta = Session.SessionFactory.GetClassMetadata(entityType);
            if (classMeta == null)
            {
                throw new ArgumentException("Metadata not found for type " + entity.GetType());
            }

            var keyValues = GetIdentifierAsArray(entity, classMeta);

            return keyValues;
        }

        /// <summary>
        ///     Allows subclasses to process entities before they are saved.  This method is called
        ///     after BeforeSaveEntities(saveMap), and before any session.Save methods are called.
        ///     The foreign-key associations on the entities have been resolved, relating the entities
        ///     to each other, and attaching proxies for other many-to-one associations.
        /// </summary>
        /// <param name="entitiesToPersist">List of entities in the order they will be saved</param>
        /// <returns>The same entitiesToPersist.  Overrides of this method may modify the list.</returns>
        public virtual List<EntityInfo> BeforeSaveEntityGraph(List<EntityInfo> entitiesToPersist)
        {
            _eventPublisher.PublishAsync(new BreezeBeforeSaveAsyncEvent(entitiesToPersist)).GetAwaiter().GetResult();

            return entitiesToPersist;
        }

        protected internal override void AfterSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap,
            List<KeyMapping> keyMappings)
        {
            _eventPublisher.PublishAsync(new BreezeAfterSaveAsyncEvent(saveMap, keyMappings)).GetAwaiter().GetResult();
        }


        public virtual async Task<List<EntityInfo>> BeforeSaveEntityGraphAsync(List<EntityInfo> entitiesToPersist)
        {
            await _eventPublisher.PublishAsync(new BreezeBeforeSaveAsyncEvent(entitiesToPersist));

            return entitiesToPersist;
        }

        protected virtual void BeforeFlush(List<EntityInfo> entitiesToPersist)
        {
            _eventPublisher.PublishAsync(new BreezeBeforeFlushAsyncEvent(entitiesToPersist)).GetAwaiter().GetResult();
        }

        protected virtual async Task BeforeFlushAsync(List<EntityInfo> entitiesToPersist)
        {
            await _eventPublisher.PublishAsync(new BreezeBeforeFlushAsyncEvent(entitiesToPersist));
        }

        protected virtual void AfterFlush(List<EntityInfo> entitiesToPersist)
        {
            _eventPublisher.PublishAsync(new BreezeAfterFlushAsyncEvent(entitiesToPersist)).GetAwaiter().GetResult();
        }

        protected virtual async Task AfterFlushAsync(List<EntityInfo> entitiesToPersist)
        {
            await _eventPublisher.PublishAsync(new BreezeAfterFlushAsyncEvent(entitiesToPersist));
        }

        protected override bool HandleSaveException(Exception e, SaveWorkState saveWorkState)
        {
            Session.RollbackTransaction();

            return base.HandleSaveException(e, saveWorkState);
        }

        /// <summary>
        ///     If TypeFilter function is defined, returns TypeFilter(entityInfo.Entity.GetType())
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <returns>true if the entity should be saved, false if not</returns>
        internal override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            if (!base.BeforeSaveEntity(entityInfo))
            {
                return false;
            }

            if (TypeFilter == null)
            {
                return true;
            }

            return TypeFilter(entityInfo.Entity.GetType());
        }

        #region Metadata

        /// <summary>
        ///     Sets a function to filter types from metadata generation and SaveChanges.
        ///     The function returns true if a Type should be included, false otherwise.
        /// </summary>
        /// <example>
        ///     // exclude the LogRecord entity
        ///     MyNHContext.TypeFilter = (type) => type.Name != "LogRecord";
        /// </example>
        /// <example>
        ///     // exclude certain entities, and all Audit* entities
        ///     var excluded = new string[] { "Comment", "LogRecord", "UserPermission" };
        ///     MyNHContext.TypeFilter = (type) =>
        ///     {
        ///     if (excluded.Contains(type.Name)) return false;
        ///     if (type.Name.StartsWith("Audit")) return false;
        ///     return true;
        ///     };
        /// </example>
        public Func<Type, bool> TypeFilter { get; set; }

        protected override string BuildJsonMetadata()
        {
            MetadataSchema metadata;
            bool isBuilt;

            lock (BuildMetadataLock)
            {
                isBuilt = IsMetadataBuilt();
                metadata = GetMetadata();
            }

            if (!isBuilt)
            {
                _eventPublisher.Publish(new BreezeMetadataBuiltEvent(metadata, Session.SessionFactory));
            }

            return JsonConvert.SerializeObject(metadata, Formatting.Indented);
        }

        protected bool IsMetadataBuilt()
        {
            return FactoryMetadata.ContainsKey(Session.SessionFactory);
        }

        protected virtual void OnMetadataBuilt(MetadataSchema metadata)
        {
        }

        protected MetadataSchema GetMetadata()
        {
            if (_metadata == null)
                lock (MetadataLock)
                {
                    if (!FactoryMetadata.TryGetValue(Session.SessionFactory, out _metadata))
                    {
                        var builder = new NHMetadataBuilder(Session.SessionFactory, _breezeConfig, _breezeConfigurator);
                        _metadata = builder.BuildMetadata(TypeFilter);
                        FactoryMetadata.Add(Session.SessionFactory, _metadata);
                        OnMetadataBuilt(_metadata);
                    }
                }

            return _metadata;
        }

        #endregion

        #region Save Changes

        private readonly Dictionary<EntityInfo, KeyMapping>
            _entityKeyMapping = new Dictionary<EntityInfo, KeyMapping>();

        private readonly List<EntityError> _entityErrors = new List<EntityError>();
        private MetadataSchema _metadata;

        /// <summary>
        ///     Persist the changes to the entities in the saveMap.
        ///     This implements the abstract method in ContextProvider.
        ///     Assigns saveWorkState.KeyMappings, which map the temporary keys to their real generated keys.
        ///     Note that this method sets session.FlushMode = FlushMode.Never, so manual flushes are required.
        /// </summary>
        /// <param name="saveMap">Map of Type -> List of entities of that type</param>
        protected override void SaveChangesCore(SaveWorkState saveWorkState)
        {
            var saveMap = saveWorkState.SaveMap;
            var flushMode = Session.FlushMode;
            Session.FlushMode = FlushMode.Manual;
            var tx = Session.Transaction;
            var hasExistingTransaction = tx.IsActive;

            if (!hasExistingTransaction)
            {
                tx.Begin(_breezeConfig.GetTransactionSettings().IsolationLevelAs);
            }

            try
            {
                // Relate entities in the saveMap to other NH entities, so NH can save the FK values.
                var fixer = GetRelationshipFixer(saveMap);
                var saveOrder = fixer.FixupRelationships();

                // Allow subclass to process entities before we save them
                AddKeyMappings(saveOrder);
                saveOrder = BeforeSaveEntityGraph(saveOrder);
                ProcessSaves(saveOrder);
                BeforeFlush(saveOrder);
                Session.Flush();
                Session.FlushMode = flushMode;
                AfterFlush(saveOrder);
                RefreshFromSession(saveMap);
                if (!hasExistingTransaction)
                {
                    tx.Commit();
                }
                fixer.RemoveRelationships();
            }
            catch (PropertyValueException pve)
            {
                // NHibernate can throw this
                if (!hasExistingTransaction) tx.Rollback();
                _entityErrors.Add(new EntityError
                {
                    EntityTypeName = pve.EntityName,
                    ErrorMessage = pve.Message,
                    ErrorName = "PropertyValueException",
                    KeyValues = null,
                    PropertyName = pve.PropertyName
                });
                saveWorkState.EntityErrors = _entityErrors;
            }
            catch (Exception)
            {
                if (!hasExistingTransaction)
                {
                    tx.Rollback();
                }
                throw;
            }
            finally
            {
                if (!hasExistingTransaction)
                {
                    tx.Dispose();
                }
            }

            saveWorkState.KeyMappings = UpdateAutoGeneratedKeys(saveWorkState.EntitiesWithAutoGeneratedKeys);
        }

        protected virtual async Task SaveChangesCoreAsync(SaveWorkState saveWorkState)
        {
            var saveMap = saveWorkState.SaveMap;
            var flushMode = Session.FlushMode;
            Session.FlushMode = FlushMode.Manual;
            var tx = Session.Transaction;
            var hasExistingTransaction = tx.IsActive;

            if (!hasExistingTransaction)
            {
                tx.Begin(_breezeConfig.GetTransactionSettings().IsolationLevelAs);
            }

            try
            {
                // Relate entities in the saveMap to other NH entities, so NH can save the FK values.
                var fixer = GetRelationshipFixer(saveMap);
                var saveOrder = fixer.FixupRelationships();
                // Allow subclass to process entities before we save them
                AddKeyMappings(saveOrder);
                saveOrder = await BeforeSaveEntityGraphAsync(saveOrder);
                await ProcessSavesAsync(saveOrder);
                await BeforeFlushAsync(saveOrder);
                await Session.FlushAsync();
                Session.FlushMode = flushMode;
                await AfterFlushAsync(saveOrder);
                await RefreshFromSessionAsync(saveMap);
                if (!hasExistingTransaction)
                {
                    await tx.CommitAsync();
                }
                fixer.RemoveRelationships();
            }
            catch (PropertyValueException pve)
            {
                // NHibernate can throw this
                if (!hasExistingTransaction) tx.Rollback();
                _entityErrors.Add(new EntityError
                {
                    EntityTypeName = pve.EntityName,
                    ErrorMessage = pve.Message,
                    ErrorName = "PropertyValueException",
                    KeyValues = null,
                    PropertyName = pve.PropertyName
                });
                saveWorkState.EntityErrors = _entityErrors;
            }
            catch (Exception)
            {
                if (!hasExistingTransaction)
                {
                    tx.Rollback();
                }
                throw;
            }
            finally
            {
                Session.FlushMode = flushMode;
                if (!hasExistingTransaction)
                {
                    tx.Dispose();
                }
            }

            saveWorkState.KeyMappings = UpdateAutoGeneratedKeys(saveWorkState.EntitiesWithAutoGeneratedKeys);
        }

        /// <summary>
        ///     Get a new NHRelationshipFixer using the saveMap and the foreign-key map from the metadata.
        /// </summary>
        /// <param name="saveMap"></param>
        /// <returns></returns>
        protected NhRelationshipFixer GetRelationshipFixer(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            // Get the map of foreign key relationships from the metadata
            var fkMap = GetMetadata().ForeignKeyMap;
            return new NhRelationshipFixer(saveMap, fkMap, Session, _breezeConfigurator);
        }

        /// <summary>
        ///     Add key mappings for entities in the saveOrder.
        /// </summary>
        /// <param name="saveOrder"></param>
        protected void AddKeyMappings(List<EntityInfo> saveOrder)
        {
            var sessionFactory = Session.SessionFactory;
            foreach (var entityInfo in saveOrder)
            {
                var entityType = Session.GetProxyRealType(entityInfo.Entity);
                var classMeta = sessionFactory.GetClassMetadata(entityType);
                AddKeyMapping(entityInfo, entityType, classMeta);
            }
        }

        /// <summary>
        ///     Persist the changes to the entities in the saveOrder.
        /// </summary>
        /// <param name="saveOrder"></param>
        protected void ProcessSaves(List<EntityInfo> saveOrder)
        {
            var sessionFactory = Session.SessionFactory;
            foreach (var entityInfo in saveOrder)
            {
                var entityType = Session.GetProxyRealType(entityInfo.Entity);
                var classMeta = sessionFactory.GetClassMetadata(entityType);
                ProcessEntity(entityInfo, classMeta);
            }
        }

        /// <summary>
        ///     Persist the changes to the entities in the saveOrder.
        /// </summary>
        /// <param name="saveOrder"></param>
        protected async Task ProcessSavesAsync(List<EntityInfo> saveOrder)
        {
            var sessionFactory = Session.SessionFactory;
            foreach (var entityInfo in saveOrder)
            {
                var entityType = Session.GetProxyRealType(entityInfo.Entity);
                var classMeta = sessionFactory.GetClassMetadata(entityType);
                await ProcessEntityAsync(entityInfo, classMeta);
            }
        }

        /// <summary>
        ///     Add, update, or delete the entity according to its EntityState.
        /// </summary>
        /// <param name="entityInfo"></param>
        protected void ProcessEntity(EntityInfo entityInfo, IClassMetadata classMeta)
        {
            var entity = entityInfo.Entity;
            var state = entityInfo.EntityState;

            // Restore the old value of the concurrency column so Hibernate will be able to save the entity
            if (classMeta.IsVersioned)
            {
                RestoreOldVersionValue(entityInfo, classMeta);
            }

            if (state == EntityState.Modified)
            {
                CheckForKeyUpdate(entityInfo, classMeta);
                Session.Update(entity);
            }
            else if (state == EntityState.Added)
            {
                Session.Save(entity);
            }
            else if (state == EntityState.Deleted)
            {
                Session.Delete(entity);
            }
        }

        /// <summary>
        ///     Add, update, or delete the entity according to its EntityState.
        /// </summary>
        /// <param name="entityInfo"></param>
        protected async Task ProcessEntityAsync(EntityInfo entityInfo, IClassMetadata classMeta)
        {
            var entity = entityInfo.Entity;
            var state = entityInfo.EntityState;

            // Restore the old value of the concurrency column so Hibernate will be able to save the entity
            if (classMeta.IsVersioned) RestoreOldVersionValue(entityInfo, classMeta);

            if (state == EntityState.Modified)
            {
                CheckForKeyUpdate(entityInfo, classMeta);
                await Session.UpdateAsync(entity);
            }
            else if (state == EntityState.Added)
            {
                await Session.SaveAsync(entity);
            }
            else if (state == EntityState.Deleted)
            {
                await Session.DeleteAsync(entity);
            }
        }

        protected void CheckForKeyUpdate(EntityInfo entityInfo, IClassMetadata classMeta)
        {
            if (classMeta.HasIdentifierProperty && entityInfo.OriginalValuesMap != null
                                                && entityInfo.OriginalValuesMap.ContainsKey(classMeta
                                                    .IdentifierPropertyName))
            {
                var errors = new EntityError[1]
                {
                    new EntityError
                    {
                        EntityTypeName = entityInfo.Entity.GetType().FullName,
                        ErrorMessage = "Cannot update part of the entity's key",
                        ErrorName = "KeyUpdateException",
                        KeyValues = GetIdentifierAsArray(entityInfo.Entity, classMeta),
                        PropertyName = classMeta.IdentifierPropertyName
                    }
                };

                throw new EntityErrorsException("Cannot update part of the entity's key", errors);
            }
        }

        /// <summary>
        ///     Restore the old value of the concurrency column so Hibernate will save the entity.
        ///     Otherwise it will complain because Breeze has already changed the value.
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="classMeta"></param>
        protected void RestoreOldVersionValue(EntityInfo entityInfo, IClassMetadata classMeta)
        {
            if (entityInfo.OriginalValuesMap == null || entityInfo.OriginalValuesMap.Count == 0) return;
            var vcol = classMeta.VersionProperty;
            var vname = classMeta.PropertyNames[vcol];
            object oldVersion;
            if (entityInfo.OriginalValuesMap.TryGetValue(vname, out oldVersion))
            {
                var entity = entityInfo.Entity;
                var vtype = classMeta.PropertyTypes[vcol].ReturnedClass;
                oldVersion = Convert.ChangeType(oldVersion, vtype); // because JsonConvert makes all integers Int64
                classMeta.SetPropertyValue(entity, vname, oldVersion);
            }
        }

        /// <summary>
        ///     Record the value of the temporary key in EntityKeyMapping
        /// </summary>
        /// <param name="entityInfo"></param>
        protected void AddKeyMapping(EntityInfo entityInfo, Type type, IClassMetadata meta)
        {
            if (entityInfo.EntityState != EntityState.Added) return;
            var entity = entityInfo.Entity;
            var id = GetIdentifier(entity, meta);
            var km = new KeyMapping {EntityTypeName = type.FullName, TempValue = id};
            _entityKeyMapping.Add(entityInfo, km);
        }

        /// <summary>
        ///     Get the identifier value for the entity.  If the entity does not have an
        ///     identifier property, or natural identifiers defined, then the entity itself is returned.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        protected object GetIdentifier(object entity, IClassMetadata meta = null)
        {
            var type = Session.GetProxyRealType(entity);
            meta = meta ?? Session.SessionFactory.GetClassMetadata(type);

            if (meta.IdentifierType != null)
            {
                var id = meta.GetIdentifier(entity);
                if (meta.IdentifierType.IsComponentType)
                {
                    var compType = (ComponentType) meta.IdentifierType;
                    return compType.GetPropertyValues(id);
                }

                return id;
            }

            if (meta.HasNaturalIdentifier)
            {
                var idprops = meta.NaturalIdentifierProperties;
                var values = meta.GetPropertyValues(entity);
                var idvalues = idprops.Select(i => values[i]).ToArray();
                return idvalues;
            }

            return entity;
        }

        /// <summary>
        ///     Get the identier value for the entity as an object[].
        ///     This is needed for creating an EntityError.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        protected object[] GetIdentifierAsArray(object entity, IClassMetadata meta)
        {
            var value = GetIdentifier(entity, meta);

            if (value.GetType().IsArray)
            {
                return (object[]) value;
            }

            return new[] {value};
        }

        /// <summary>
        ///     Update the KeyMappings with their real values.
        /// </summary>
        /// <returns></returns>
        protected List<KeyMapping> UpdateAutoGeneratedKeys(List<EntityInfo> entitiesWithAutoGeneratedKeys)
        {
            var list = new List<KeyMapping>();
            foreach (var entityInfo in entitiesWithAutoGeneratedKeys)
            {
                KeyMapping km;
                if (_entityKeyMapping.TryGetValue(entityInfo, out km))
                    if (km.TempValue != null)
                    {
                        var entity = entityInfo.Entity;
                        var id = GetIdentifier(entity);
                        km.RealValue = id;
                        list.Add(km);
                    }
            }

            return list;
        }

        /// <summary>
        ///     Refresh the entities from the database.  This picks up changes due to triggers, etc.
        /// </summary>
        /// <param name="saveMap"></param>
        protected void RefreshFromSession(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            //using (var tx = session.BeginTransaction()) {
            foreach (var kvp in saveMap)
            {
                var config = _breezeConfigurator.GetModelConfiguration(kvp.Key);
                if (!config.RefreshAfterSave && !config.RefreshAfterUpdate)
                {
                    continue;
                }

                foreach (var entityInfo in kvp.Value)
                {
                    if (entityInfo.EntityState == EntityState.Added && config.RefreshAfterSave ||
                        entityInfo.EntityState == EntityState.Modified && config.RefreshAfterUpdate)
                    {
                        Session.Refresh(entityInfo.Entity);
                    }
                }
            }

            //tx.Commit();
            //}
        }

        /// <summary>
        ///     Refresh the entities from the database.  This picks up changes due to triggers, etc.
        /// </summary>
        /// <param name="saveMap"></param>
        protected async Task RefreshFromSessionAsync(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            //using (var tx = session.BeginTransaction()) {
            foreach (var kvp in saveMap)
            {
                var config = _breezeConfigurator.GetModelConfiguration(kvp.Key);
                if (!config.RefreshAfterSave && !config.RefreshAfterUpdate)
                {
                    continue;
                }

                foreach (var entityInfo in kvp.Value)
                {
                    if (entityInfo.EntityState == EntityState.Added && config.RefreshAfterSave ||
                        entityInfo.EntityState == EntityState.Modified && config.RefreshAfterUpdate)
                    {
                        await Session.RefreshAsync(entityInfo.Entity);
                    }
                }
            }

            //tx.Commit();
            //}
        }

        #endregion
    }
}
