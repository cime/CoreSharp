using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreSharp.Breeze.Comparers;
using CoreSharp.Breeze.Extensions;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Hql;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using NHibernate.Type;

namespace CoreSharp.Breeze
{
    /// <summary>
    /// Utility class for re-establishing the relationships between entities prior to saving them in Nhibernate.
    ///
    /// Breeze requires many-to-one relationships to have properties both the related entity and its ID, and it
    /// sends only the ID in the save bundle.  To make it work with NH, we map the <code>many-to-one</code> entity, and map the
    /// foreign key ID with <code> insert="false" update="false" </code>, so the <code>many-to-one</code> entity must
    /// be populated in order for the foreign key value to be saved in the DB.  To work
    /// around this problem, this class uses the IDs sent by Breeze to re-connect the related entities.
    /// </summary>
    public class NhRelationshipFixer
    {
        private readonly Dictionary<Type, List<EntityInfo>> _saveMap;
        private readonly IDictionary<string, string> _fkMap;
        private readonly ISession _session;
        private List<EntityInfo> _saveOrder;
        private List<EntityInfo> _deleteOrder;
        private readonly Dictionary<EntityInfo, List<EntityInfo>> _dependencyGraph;
        private bool _removeMode;
        private readonly Dictionary<EntityInfo, object> _clientEntityObjects = new Dictionary<EntityInfo, object>();
        private readonly IBreezeConfigurator _breezeConfigurator;
        private readonly BreezeContext _breezeContext;

        private readonly Regex _removeIdSuffixRegex = new Regex("Id$", RegexOptions.Compiled);

        /// <summary>
        /// Create new instance with the given saveMap and fkMap.  Since the saveMap is unique per save,
        /// this instance will be useful for processing one entire save bundle only.
        /// </summary>
        /// <param name="saveMap">Map of entity types -> entity instances to save.  This is provided by Breeze in the SaveChanges call.</param>
        /// <param name="fkMap">Map of relationship name -> foreign key name.  This is built in the NHBreezeMetadata class.</param>
        /// <param name="session">NHibernate session that will save the entities</param>
        /// <param name="breezeConfigurator"></param>
        /// <param name="breezeContext"></param>
        public NhRelationshipFixer(Dictionary<Type, List<EntityInfo>> saveMap, IDictionary<string, string> fkMap,
            ISession session,
            IBreezeConfigurator breezeConfigurator, BreezeContext breezeContext)
        {
            _saveMap = saveMap;
            _fkMap = fkMap;
            _session = session;
            _breezeConfigurator = breezeConfigurator;
            _breezeContext = breezeContext;
            _dependencyGraph = new Dictionary<EntityInfo, List<EntityInfo>>();
        }

        /// <summary>
        /// Connect the related entities in the saveMap to other entities.  If the related entities
        /// are not in the saveMap, they are loaded from the session.
        /// </summary>
        /// <returns>The list of entities in the order they should be save, according to their relationships.</returns>
        public List<EntityInfo> FixupRelationships()
        {
            _removeMode = false;
            ProcessRelationships();
            return SortDependencies();
        }

        /// <summary>
        /// Remove the navigations between entities in the saveMap.  This flattens the JSON
        /// result so Breeze can handle it.
        /// </summary>
        /// <param name="saveMap">Map of entity types -> entity instances to save</param>
        public void RemoveRelationships()
        {
            _removeMode = true;
            ProcessRelationships();
        }

        /// <summary>
        /// Add the relationship to the dependencyGraph
        /// </summary>
        /// <param name="child">Entity that depends on parent (e.g. has a many-to-one relationship to parent)</param>
        /// <param name="parent">Entity that child depends on (e.g. one parent has one-to-many children)</param>
        /// <param name="removeReverse">True to find and remove the reverse relationship.  Used for handling one-to-ones.</param>
        private void AddToGraph(EntityInfo child, EntityInfo parent, bool removeReverse)
        {
            List<EntityInfo> list;
            if (!_dependencyGraph.TryGetValue(child, out list))
            {
                list = new List<EntityInfo>(5);
                _dependencyGraph.Add(child, list);
            }
            if (parent != null) list.Add(parent);

            if (removeReverse)
            {
                List<EntityInfo> parentList;
                if (_dependencyGraph.TryGetValue(parent, out parentList))
                {
                    parentList.Remove(child);
                }
            }
        }

        /// <summary>
        /// Sort the entries in the dependency graph according to their dependencies.
        /// </summary>
        /// <returns></returns>
        private List<EntityInfo> SortDependencies()
        {
            _saveOrder = new List<EntityInfo>();
            _deleteOrder = new List<EntityInfo>();
            foreach (var entityInfo in _dependencyGraph.Keys)
            {
                AddToSaveOrder(entityInfo, 0);
            }
            _deleteOrder.Reverse();
            _saveOrder.AddRange(_deleteOrder);
            return _saveOrder;
        }

        /// <summary>
        /// Recursively add entities to the saveOrder or deleteOrder according to their dependencies
        /// </summary>
        /// <param name="entityInfo">Entity to be added.  Its dependencies will be added depth-first.</param>
        /// <param name="depth">prevents infinite recursion in case of cyclic dependencies</param>
        private void AddToSaveOrder(EntityInfo entityInfo, int depth)
        {
            if (_saveOrder.Contains(entityInfo)) return;
            if (_deleteOrder.Contains(entityInfo)) return;
            if (depth > 10) return;

            var dependencies = _dependencyGraph[entityInfo];
            foreach (var dep in dependencies)
            {
                AddToSaveOrder(dep, depth + 1);
            }

            if (entityInfo.EntityState == EntityState.Deleted)
            {
                if (!_deleteOrder.Contains(entityInfo))
                {
                    _deleteOrder.Add(entityInfo);
                }
            }
            else
            {
                if (!_saveOrder.Contains(entityInfo))
                {
                    _saveOrder.Add(entityInfo);
                }
            }
        }

        /// <summary>
        /// Add or remove the entity relationships according to the removeMode.
        /// </summary>
        private void ProcessRelationships()
        {
            //Before process we have to manipulate EntityInfo
            foreach (var kvp in _saveMap)
            {
                var entityType = kvp.Key;
                var classMeta = _session.SessionFactory.GetClassMetadata(entityType);

                //Modify EntityInfos
                foreach (var entityInfo in kvp.Value)
                {
                    if (_removeMode)
                    {
                        SetupEntityInfoForSerialization(entityType, entityInfo, classMeta);
                    }
                    else
                    {
                        SetupEntityInfoForSaving(entityType, entityInfo, classMeta);
                    }
                }
            }

            foreach (var kvp in _saveMap)
            {
                var entityType = kvp.Key;
                var classMeta = _session.SessionFactory.GetClassMetadata(entityType);

                foreach (var entityInfo in kvp.Value)
                {
                    AddToGraph(entityInfo, null, false); // make sure every entity is in the graph
                    if (!_removeMode)
                        FixupRelationships(entityInfo, classMeta);
                }
            }
        }

        private bool SetupEntityInfoForSerialization(Type entityType, EntityInfo entityInfo, IClassMetadata meta)
        {
            if (!_removeMode || !_clientEntityObjects.ContainsKey(entityInfo)) return false;

            var clientEntity = _clientEntityObjects[entityInfo];
            var id = meta.GetIdentifier(entityInfo.Entity);
            meta.SetIdentifier(clientEntity, id);

            //We have to set the properties from the client object
            var propNames = meta.PropertyNames;
            var propTypes = meta.PropertyTypes;

            using (var childSession = _session.SessionWithOptions().Connection().OpenSession())
            {
                for (var i = 0; i < propNames.Length; i++)
                {
                    var propType = propTypes[i];
                    var propName = propNames[i];
                    if (propType is IAssociationType associationType)
                    {
                        if (entityInfo.UnmappedValuesMap != null && entityInfo.UnmappedValuesMap.ContainsKey(propName + "Id"))
                        {
                            var associatedEntityName = associationType.GetAssociatedEntityName((ISessionFactoryImplementor) _session.SessionFactory);
                            var associatedEntityMetadata = _session.SessionFactory.GetClassMetadata(associatedEntityName);
                            var associatedEntityValue = GetPropertyValue(meta, entityInfo.Entity, propName);
                            if (associatedEntityValue != null)
                            {
                                clientEntity.SetMemberValue(propName, childSession.Load(associatedEntityName, GetPropertyValue(associatedEntityMetadata,associatedEntityValue, null)));
                            }
                        }
                    }
                    else if (propType.IsComponentType)
                    {
                        var compType = (ComponentType)propType;
                        var compPropNames = compType.PropertyNames;
                        var compPropTypes = compType.Subtypes;
                        var component = GetPropertyValue(meta, entityInfo.Entity, propName);
                        var compValues = compType.GetPropertyValues(component);
                        for (var j = 0; j < compPropNames.Length; j++)
                        {
                            var compPropType = compPropTypes[j];
                            if (!compPropType.IsAssociationType) continue;
                            compValues[j] = null;
                        }
                        var clientCompVal = GetPropertyValue(meta, clientEntity, propName);
                        compType.SetPropertyValues(clientCompVal, compValues);
                    }
                    else
                    {
                        var val = meta.GetPropertyValue(entityInfo.Entity, propName);
                        meta.SetPropertyValue(clientEntity, propName, val);
                    }
                }
            }
            // TODO: update unmapped properties
            entityInfo.Entity = clientEntity;
            return true;
        }

        private bool SetupEntityInfoForSaving(Type entityType, EntityInfo entityInfo, IClassMetadata meta)
        {
            var id = meta.GetIdentifier(entityInfo.Entity);
            var sessionImpl = _session.GetSessionImplementation();
            object dbEntity;
            string[] propNames;

            if (entityInfo.EntityState == EntityState.Added)
            {
                //meta.Instantiate(id) -> Instantiate method can create a proxy when formulas are present. Saving non persistent proxies will throw an exception
                dbEntity = Activator.CreateInstance(entityType, true);
                meta.SetIdentifier(dbEntity, id);
            }
            else
            {
                //dbEntity = session.Get(entityType, id); Get is not good as it can return a proxy
                if (meta.IdentifierType.IsComponentType)
                {
                    // for entities with composite key the identifier is the entity itself
                    // we need to create a copy as ImmediateLoad will fill the given entity
                    var componentType = (ComponentType) meta.IdentifierType;
                    dbEntity = Activator.CreateInstance(entityType, true);

                    // We need to check if the primary key was changed
                    var oldKeyValues = new object[componentType.PropertyNames.Length];
                    var keyModified = false;
                    for (var i = 0; i < componentType.PropertyNames.Length; i++)
                    {
                        var propName = componentType.PropertyNames[i];
                        if (entityInfo.OriginalValuesMap.ContainsKey(propName))
                        {
                            oldKeyValues[i] = entityInfo.OriginalValuesMap[propName];
                            keyModified = true;
                        }
                        else
                        {
                            oldKeyValues[i] = componentType.GetPropertyValue(entityInfo.Entity, i);
                        }
                    }

                    componentType.SetPropertyValues(dbEntity, oldKeyValues);
                    dbEntity = sessionImpl.ImmediateLoad(entityType.FullName, dbEntity);

                    // As NHibernate does not support updating the primary key we need to do it manually using hql
                    if (keyModified)
                    {
                        var newKeyValues = componentType.GetPropertyValues(entityInfo.Entity);
                        var parameters = new Dictionary<string, KeyValuePair<object, IType>>();
                        var setStatement = "set ";
                        var whereStatement = "where ";
                        for (var i = 0; i < componentType.PropertyNames.Length; i++)
                        {
                            if (i > 0)
                            {
                                setStatement += ", ";
                                whereStatement += " and ";
                            }
                            var propName = componentType.PropertyNames[i];
                            var paramName = string.Format("new{0}", propName);
                            setStatement += string.Format("{0}=:{1}", propName, paramName);
                            parameters.Add(paramName, new KeyValuePair<object, IType>(newKeyValues[i], componentType.Subtypes[i]));

                            paramName = string.Format("old{0}", propName);
                            whereStatement += string.Format("{0}=:{1}", propName, paramName);
                            parameters.Add(paramName, new KeyValuePair<object, IType>(oldKeyValues[i], componentType.Subtypes[i]));

                        }
                        var updateQuery = sessionImpl.CreateQuery(new StringQueryExpression(string.Format("update {0} {1} {2}", entityType.Name, setStatement, whereStatement)));
                        foreach (var pair in parameters)
                        {
                            updateQuery.SetParameter(pair.Key, pair.Value.Key, pair.Value.Value);
                        }
                        var count = updateQuery.ExecuteUpdate();
                        if (count != 1)
                        {
                            throw new InvalidOperationException(string.Format("Query for updating composite key updated '{0}' rows instead of '1'", count));
                        }
                        componentType.SetPropertyValues(dbEntity, componentType.GetPropertyValues(entityInfo.Entity));
                    }
                }
                else
                {
                    dbEntity = sessionImpl.ImmediateLoad(entityType.FullName, id);
                }

                //dbEntity = session.Get(entityType, id, LockMode.None);
            }


            if (dbEntity == null)
            {
                throw new NullReferenceException(string.Format("Entity of type '{0}' with id '{1}' does not exists in database",
                    entityType.FullName, id));
            }

            //var modelConfig = BreezeModelConfigurator.GetModelConfiguration(entityType);

            //Save the original client object
            _clientEntityObjects[entityInfo] = entityInfo.Entity;

            var entityInfoEntity = entityInfo.Entity;
            entityInfo.Entity = dbEntity;
            _breezeContext.BeforeModify(entityInfo);


            //We have to set the properties from the client object
            propNames = meta.PropertyNames;
            var propTypes = meta.PropertyTypes;

            var config = _breezeConfigurator.GetModelConfiguration(entityType);

            // TODO: set only modified properties
            for (var i = 0; i < propNames.Length; i++)
            {
                var propType = propTypes[i];
                var propName = propNames[i];

                var memberConfig = config.MemberConfigurations.ContainsKey(propName)
                    ? config.MemberConfigurations[propName]
                    : null;
                if (memberConfig != null && (
                    (memberConfig.Ignored.HasValue && memberConfig.Ignored.Value) ||
                    (memberConfig.Writable.HasValue && !memberConfig.Writable.Value) ||
                    (memberConfig.ShouldDeserializePredicate != null && memberConfig.ShouldDeserializePredicate.Invoke(entityInfoEntity) == false)
                ))
                    continue;

                if(propType.IsAssociationType)
                    continue;

                if (propType.IsComponentType)
                {
                    var compType = (ComponentType) propType;
                    var componentVal = GetPropertyValue(meta, entityInfoEntity, propName);
                    var dbComponentVal = GetPropertyValue(meta, dbEntity, propName);

                    if (dbComponentVal == null)
                    {
                        dbEntity.SetMemberValue(propName, componentVal);
                    }
                    else
                    {
                        var compPropsVal = compType.GetPropertyValues(componentVal);
                        compType.SetPropertyValues(dbComponentVal, compPropsVal);
                    }
                }
                else
                {
                    var index = Array.IndexOf(meta.PropertyNames, propName);
                    var isLazy = meta.PropertyLaziness[index];
                    var isNullable = meta.PropertyNullability[index];
                    var val = meta.GetPropertyValue(entityInfoEntity, propName);

                    if (!isLazy || !isNullable || !(val is byte[]) || !(new byte[] { 0,0,0,0,0,0,39,117 }.SequenceEqual((byte[])val)));
                    {
                        meta.SetPropertyValue(dbEntity, propName, val);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Connect the related entities based on the foreign key values.
        /// Note that this may cause related entities to be loaded from the DB if they are not already in the session.
        /// </summary>
        /// <param name="entityInfo">Entity that will be saved</param>
        /// <param name="meta">Metadata about the entity type</param>
        private void FixupRelationships(EntityInfo entityInfo, IClassMetadata meta)
        {
            var propNames = meta.PropertyNames;
            var propTypes = meta.PropertyTypes;

            if (meta.IdentifierType != null)
            {
                var propType = meta.IdentifierType;
                if (propType.IsAssociationType && propType.IsEntityType)
                {
                    FixupRelationship(meta.IdentifierPropertyName, (EntityType)propType, entityInfo, meta);
                }
                else if (propType.IsComponentType)
                {
                    FixupComponentRelationships(meta.IdentifierPropertyName, (ComponentType)propType, entityInfo, meta);
                }
            }

            for (var i = 0; i < propNames.Length; i++)
            {
                var propType = propTypes[i];
                if (propType.IsAssociationType && propType.IsEntityType)
                {
                    FixupRelationship(propNames[i], (EntityType)propTypes[i], entityInfo, meta);
                    var oneToManyType = propType as ManyToOneType;
                    if (oneToManyType != null && !_removeMode)
                        TryToFindAndLinkBidirectionalRelation(oneToManyType, (EntityType)propTypes[i], propNames[i], entityInfo, meta);
                }
                else if (propType.IsComponentType)
                {
                    FixupComponentRelationships(propNames[i], (ComponentType)propType, entityInfo, meta);
                }
                else if (propType.IsAssociationType && propType.IsCollectionType && _removeMode)
                {
                    meta.SetPropertyValue(entityInfo.Entity, propNames[i], null);
                }
            }
        }

        /// <summary>
        /// Find out if a bidirectional relation exists, if exists then add entity to the collection
        /// </summary>
        /// <param name="manyToOneType">Many to one relation</param>
        /// <param name="childEntityType">Entity type of the child</param>
        /// <param name="propertyName">Property name of the ManyToOne realtion</param>
        /// <param name="childEntityInfo">Entity info of the child</param>
        /// <param name="childMeta">Child metadata</param>
        /// <returns>true if the child entity was added or removed from the parent entity collection, otherwise false</returns>
        private bool TryToFindAndLinkBidirectionalRelation(ManyToOneType manyToOneType,
            EntityType childEntityType, string propertyName,
            EntityInfo childEntityInfo, IClassMetadata childMeta)
        {
            var fk = FindForeignKey(propertyName, childMeta);
            var childPersister = childMeta as AbstractEntityPersister;
            // TODO: workaround for naming conventions where column/fk name != property name
            fk = childPersister.GetPropertyColumnNames(_removeIdSuffixRegex.Replace(fk, "")).Single();

            var parentMeta = _session.SessionFactory.GetClassMetadata(manyToOneType.ReturnedClass);
            var propNames = parentMeta.PropertyNames;
            var propTypes = parentMeta.PropertyTypes;
            if (childPersister == null)
            {
                return false;
            }
            var colProp = new Dictionary<string, string>();
            for (var i = 0; i < childPersister.PropertyNames.Length; i++)
            {
                var propName = childPersister.PropertyNames[i];
                var propType = childPersister.GetPropertyType(propName);
                if (propType.IsAssociationType)
                {
                    continue;
                }
                foreach (var col in childPersister.GetPropertyColumnNames(i))
                {
                    if(col == null)
                        continue; // formula
                    if (!colProp.ContainsKey(col))
                        colProp.Add(col, propName);
                }
            }
            if (childPersister.IdentifierType.IsComponentType)
            {
                var componentType = (ComponentType) childPersister.IdentifierType;
                for (var i = 0; i < componentType.PropertyNames.Length; i++)
                {
                    colProp.Add(childPersister.IdentifierColumnNames[i], componentType.PropertyNames[i]);
                }
            }
            else
            {
                foreach (var col in childPersister.IdentifierColumnNames)
                {
                    colProp.Add(col, childPersister.IdentifierPropertyName);
                }
            }


            for (var i = 0; i < propNames.Length; i++)
            {
                var propType = propTypes[i];
                var propName = propNames[i];
                if (!propType.IsAssociationType || !propType.IsCollectionType) continue;
                var colType = (CollectionType)propType;

                var refCols = colType.GetReferencedColumns(_session.GetSessionImplementation().Factory);
                var refProps = refCols.Select(refCol => !colProp.ContainsKey(refCol) ? refCol : colProp[refCol]).ToArray();
                if (NHMetadataBuilder.CatColumnNames(refProps) != fk)
                    continue;

                var elmType = colType.GetElementType(_session.GetSessionImplementation().Factory);
                if (!elmType.ReturnedClass.IsAssignableFrom(childMeta.MappedClass))
                    continue;

                var parentEntity = GetRelatedEntity(propertyName, childEntityType, childEntityInfo, childMeta);
                if (parentEntity == null)
                    return false;

                var coll = parentMeta.GetPropertyValue(parentEntity, propName) as IEnumerable;

                if (coll == null) //Should happen only if the parent entity is not in db
                {
                    //TODO: instantiate collection
                    continue;
                }
                var collType = coll.GetType();

                //Initialize collection in order to prevent flushing
                if(!NHibernateUtil.IsInitialized(coll))
                    NHibernateUtil.Initialize(coll);

                if (colType.ReturnedClass.IsGenericType)
                {
                    var state = childEntityInfo.EntityState;
                    if (_saveMap.ContainsKey(manyToOneType.ReturnedClass))
                    {
                        var parentEntityInfo =
                            _saveMap[manyToOneType.ReturnedClass].FirstOrDefault(o => o.Entity == parentEntity);
                        if (parentEntityInfo != null)
                        {
                            switch (parentEntityInfo.EntityState)
                            {
                                case EntityState.Added:
                                    //if the parent is added then we need to add child to the collection even if the it is only updated or unmodified
                                    if (state != EntityState.Deleted)
                                    {
                                        state = EntityState.Added;
                                    }
                                    break;
                                case EntityState.Deleted:
                                    //TODO: check for cascade option
                                    break;
                                case EntityState.Modified:
                                    break;
                            }
                        }
                    }

                    //TODO: check if parent is new
                    switch (state)
                    {
                        case EntityState.Deleted:
                            var removeMethod = collType.GetMethod("Remove") ??
                                               collType.GetInterfaces()
                                                   .Select(o => o.GetMethod("Remove"))
                                                   .FirstOrDefault(o => o != null);
                            var removed = false;
                            if (removeMethod != null)
                            {
                                IEqualityComparer comparer = new EntityComparer(childMeta);

                                foreach (var item in coll)
                                {
                                    if (comparer.Equals(item, childEntityInfo.Entity))
                                    {
                                        removed = (bool)removeMethod.Invoke(coll, new[] { item });
                                        break;
                                    }
                                }
                            }
                            childMeta.SetPropertyValue(childEntityInfo.Entity, propertyName, null);//Remove relation on both sides
                            return removed;
                        case EntityState.Added:
                            var addMethod = collType.GetMethod("Add") ??
                                            collType.GetInterfaces()
                                                .Select(o => o.GetMethod("Add"))
                                                .FirstOrDefault(o => o != null);
                            if (addMethod != null)
                            {
                                addMethod.Invoke(coll, new[] { childEntityInfo.Entity });
                                return true;
                            }
                            break;
                    }
                }
                //TODO: non generic collections
            }
            return false;
        }

        /// <summary>
        /// Connect the related entities based on the foreign key values found in a component type.
        /// This updates the values of the component's properties.
        /// </summary>
        /// <param name="propName">Name of the (component) property of the entity.  May be null if the property is the entity's identifier.</param>
        /// <param name="compType">Type of the component</param>
        /// <param name="entityInfo">Breeze EntityInfo</param>
        /// <param name="meta">Metadata for the entity class</param>
        private void FixupComponentRelationships(string propName, ComponentType compType, EntityInfo entityInfo, IClassMetadata meta)
        {
            var compPropNames = compType.PropertyNames;
            var compPropTypes = compType.Subtypes;
            object component = null;
            object[] compValues = null;
            var isChanged = false;
            for (var j = 0; j < compPropNames.Length; j++)
            {
                var compPropType = compPropTypes[j];
                if (compPropType.IsAssociationType && compPropType.IsEntityType)
                {
                    if (compValues == null)
                    {
                        // get the value of the component's subproperties
                        component = GetPropertyValue(meta, entityInfo.Entity, propName);
                        compValues = compType.GetPropertyValues(component);
                    }
                    if (compValues[j] == null)
                    {
                        // the related entity is null
                        var relatedEntity = GetRelatedEntity(compPropNames[j], (EntityType)compPropType, entityInfo, meta);
                        if (relatedEntity != null)
                        {
                            compValues[j] = relatedEntity;
                            isChanged = true;
                        }
                    }
                    else if (_removeMode)
                    {
                        // remove the relationship
                        compValues[j] = null;
                        isChanged = true;
                    }
                }
            }
            if (isChanged)
            {
                compType.SetPropertyValues(component, compValues);
            }

        }

        /// <summary>
        /// Set an association value based on the value of the foreign key.  This updates the property of the entity.
        /// </summary>
        /// <param name="propName">Name of the navigation/association property of the entity, e.g. "Customer".  May be null if the property is the entity's identifier.</param>
        /// <param name="propType">Type of the property</param>
        /// <param name="entityInfo">Breeze EntityInfo</param>
        /// <param name="meta">Metadata for the entity class</param>
        private void FixupRelationship(string propName, EntityType propType, EntityInfo entityInfo, IClassMetadata meta)
        {
            var entity = entityInfo.Entity;
            if (_removeMode)
            {
                var foreignKeyName = FindForeignKey(propName, meta);
                var id = GetForeignKeyValue(entityInfo, meta, foreignKeyName);
                meta.SetPropertyValue(entity, propName, null);
                if (id != null)
                {
                    meta.SetPropertyValue(entity, foreignKeyName, id); //Set the foreigenkey as the foreign key property may be computed (resets when the relation is set)
                }

                return;
            }
            //object relatedEntity = GetPropertyValue(meta, entity, propName);
            //if (relatedEntity != null) return;    // entities are already connected

            var relatedEntity = GetRelatedEntity(propName, propType, entityInfo, meta);

            //if (relatedEntity != null) Unset if the synthetic property is not set
            meta.SetPropertyValue(entity, propName, relatedEntity);
        }

        /// <summary>
        /// Get a related entity based on the value of the foreign key.  Attempts to find the related entity in the
        /// saveMap; if its not found there, it is loaded via the Session (which should create a proxy, not actually load
        /// the entity from the database).
        /// Related entities are Promoted in the saveOrder according to their state.
        /// </summary>
        /// <param name="propName">Name of the navigation/association property of the entity, e.g. "Customer".  May be null if the property is the entity's identifier.</param>
        /// <param name="propType">Type of the property</param>
        /// <param name="entityInfo">Breeze EntityInfo</param>
        /// <param name="meta">Metadata for the entity class</param>
        /// <returns></returns>
        private object GetRelatedEntity(string propName, EntityType propType, EntityInfo entityInfo, IClassMetadata meta)
        {
            object relatedEntity = null;
            var foreignKeyName = FindForeignKey(propName, meta);
            var id = GetForeignKeyValue(entityInfo, meta, foreignKeyName);

            if (id != null)
            {
                var relatedEntityInfo = FindInSaveMap(propType.ReturnedClass, id);

                if (relatedEntityInfo == null)
                {
                    var state = entityInfo.EntityState;
                    if (state != EntityState.Deleted || !propType.IsNullable)
                    {
                        var relatedEntityName = propType.Name;
                        relatedEntity = _session.Load(relatedEntityName, id, LockMode.None);
                    }
                }
                else
                {
                    var removeReverseRelationship = propType.UseLHSPrimaryKey;
                    /*
                    var entityPersister = ((AbstractEntityPersister) meta);
                    var propIndx = entityPersister.GetPropertyIndex(propName);
                    var cascade = entityPersister.GetCascadeStyle(propIndx);
                    if (cascade == CascadeStyle.None || )
                    */
                    AddToGraph(entityInfo, relatedEntityInfo, removeReverseRelationship);
                    relatedEntity = relatedEntityInfo.Entity;
                }
            }
            return relatedEntity;
        }

        /// <summary>
        /// Find a foreign key matching the given property, by looking in the fkMap.
        /// The property may be defined on the class or a superclass, so this function calls itself recursively.
        /// </summary>
        /// <param name="propName">Name of the property e.g. "Product"</param>
        /// <param name="meta">Class metadata, for traversing the class hierarchy</param>
        /// <returns>The name of the foreign key, e.g. "ProductID"</returns>
        private string FindForeignKey(string propName, IClassMetadata meta)
        {
            var relKey = meta.EntityName + '.' + propName;
            if (_fkMap.ContainsKey(relKey))
            {
                return _fkMap[relKey];
            }
            else if (meta.IsInherited && meta is AbstractEntityPersister)
            {
                var superEntityName = ((AbstractEntityPersister)meta).MappedSuperclass;
                var superMeta = _session.SessionFactory.GetClassMetadata(superEntityName);
                return FindForeignKey(propName, superMeta);
            }
            else
            {
                throw new ArgumentException("Foreign Key '" + relKey + "' could not be found.");
            }
        }

        /// <summary>
        /// Get the value of the foreign key property.  This comes from the entity, but if that value is
        /// null, and the entity is deleted, we try to get it from the originalValuesMap.
        /// </summary>
        /// <param name="entityInfo">Breeze EntityInfo</param>
        /// <param name="meta">Metadata for the entity class</param>
        /// <param name="foreignKeyName">Name of the foreign key property of the entity, e.g. "CustomerID"</param>
        /// <returns></returns>
        private object GetForeignKeyValue(EntityInfo entityInfo, IClassMetadata meta, string foreignKeyName)
        {
            var entity = entityInfo.Entity;
            object id = null;
            if (foreignKeyName == meta.IdentifierPropertyName)
                id = meta.GetIdentifier(entity);
            else if (meta.PropertyNames.Contains(foreignKeyName))
                id = meta.GetPropertyValue(entity, foreignKeyName);
            else if (meta.IdentifierType.IsComponentType)
            {
                // compound key
                var compType = meta.IdentifierType as ComponentType;
                var index = Array.IndexOf<string>(compType.PropertyNames, foreignKeyName);
                if (index >= 0)
                {
                    var idComp = meta.GetIdentifier(entity);
                    id = compType.GetPropertyValue(idComp, index);
                }
            }

            if (id == null && entityInfo.EntityState == EntityState.Deleted)
            {
                entityInfo.OriginalValuesMap.TryGetValue(foreignKeyName, out id);
            }

            var syntheticProperties = _session.SessionFactory.GetSyntheticProperties();

            // If id is still null try get from a unmapped values (synthetic properties)
            if (entityInfo.UnmappedValuesMap == null || !entityInfo.UnmappedValuesMap.ContainsKey(foreignKeyName) || syntheticProperties == null ||
                entityInfo.Entity == null)
            {
                return id;
            }
            var entityType = entityInfo.Entity.GetType();
            if (!syntheticProperties.ContainsKey(entityType))
            {
                return id;
            }
            var synColumn = syntheticProperties[entityType].FirstOrDefault(o => o.Name == foreignKeyName);
            if (synColumn == null)
            {
                return id;
            }
            // We have to convert the value in the propriate type
            id = entityInfo.UnmappedValuesMap[foreignKeyName];

            if (id != null)
            {
                // Here we first convert id to string so that we wont get exception like: Cannot convert from Int64 to Int32
                id = Convert.ChangeType(id, synColumn.PkType.ReturnedClass, CultureInfo.InvariantCulture);
            }

            return id;
        }

        /// <summary>
        /// Return the property value for the given entity.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entity"></param>
        /// <param name="propName">If null, the identifier property will be returned.</param>
        /// <returns></returns>
        private object GetPropertyValue(IClassMetadata meta, object entity, string propName)
        {
            if (propName == null || propName == meta.IdentifierPropertyName)
                return meta.GetIdentifier(entity);
            else
                return meta.GetPropertyValue(entity, propName);
        }


        /// <summary>
        /// Find the matching entity in the saveMap.  This is for relationship fixup.
        /// </summary>
        /// <param name="entityType">Type of entity, e.g. Order.  The saveMap will be searched for this type and its subtypes.</param>
        /// <param name="entityId">Key value of the entity</param>
        /// <returns>The entity, or null if not found</returns>
        private EntityInfo FindInSaveMap(Type entityType, object entityId)
        {
            var entityInfoList = _saveMap.Where(p => entityType.IsAssignableFrom(p.Key)).SelectMany(p => p.Value).ToList();
            if (entityInfoList != null && entityInfoList.Count != 0)
            {
                var entityIdString = entityId.ToString();
                var meta = _session.SessionFactory.GetClassMetadata(entityType);
                foreach (var entityInfo in entityInfoList)
                {
                    var entity = entityInfo.Entity;
                    var id = meta.GetIdentifier(entity);
                    if (id != null && entityIdString.Equals(id.ToString()))
                        return entityInfo;
                }
            }
            return null;
        }

    }
}
