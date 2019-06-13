using System.Linq;

namespace CoreSharp.DataAccess
{
    /// <summary>
    /// The main runtime interface between a .NET application and NHibernate. This is the central
    /// API class abstracting the notion of a persistence service.
    /// </summary>
    public interface IDbStore
    {
        IQueryable<T> Query<T>() where T : IEntity;
        /// <summary>
        /// Query entities in local cache
        /// </summary>
        IQueryable<T> QueryLocal<T>() where T : IEntity;

        /// <summary>
        /// Return the persistent instance of the given entity class with the given identifier,
        /// obtaining the specified lock mode.
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">A valid identifier of an existing persistent instance of the class</param>
        /// <returns>the persistent instance</returns>
        T Load<T>(object id) where T : IEntity;

        /// <summary>
        /// Return the persistent instance of the given entity type with the given identifier,
        /// or null if there is no such persistent instance. (If the instance, or a proxy for the
        /// instance, is already associated with the session, return that instance or proxy.)
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">an identifier</param>
        /// <returns>a persistent instance or null</returns>
        T Get<T>(object id) where T : IEntity;

        /// <summary>
        /// Returns entity from ISession PersistenceContext (not yet persisted), if null it returns Get<T>(id)
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">an identifier</param>
        /// <returns>a persistent instance or null</returns>
        T Find<T>(object id) where T : class, IEntity;

        /// <summary>
        /// Either <c>Save()</c> or <c>Update()</c> the given instance, depending upon the value of
        /// its identifier property.
        /// </summary>
        /// <remarks>
        /// By default the instance is always saved. This behaviour may be adjusted by specifying
        /// an <c>unsaved-value</c> attribute of the identifier property mapping
        /// </remarks>
        /// <param name="model">A transient instance containing new or updated state</param>
        void Save(IEntity model);

        /// <summary>
        /// Remove a persistent instance from the datastore.
        /// </summary>
        /// <remarks>
        /// The argument may be an instance associated with the receiving <c>ISession</c> or a
        /// transient instance with an identifier associated with existing persistent state.
        /// </remarks>
        /// <param name="model">The instance to be removed</param>
        void Delete(IEntity model);

        /// <summary>
        /// Re-read the state of the given instance from the underlying database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is inadvisable to use this to implement long-running sessions that span many
        /// business tasks. This method is, however, useful in certain special circumstances.
        /// </para>
        /// <para>
        /// For example,
        /// <list>
        ///		<item>Where a database trigger alters the object state upon insert or update</item>
        ///		<item>After executing direct SQL (eg. a mass update) in the same session</item>
        ///		<item>After inserting a <c>Blob</c> or <c>Clob</c></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="model">A persistent instance</param>
        void Refresh(IEntity model);

        /// <summary>
        /// Get the entity instance underlying the given proxy, throwing
        /// an exception if the proxy is uninitialized. If the given object
        /// is not a proxy, simply return the argument.
        /// </summary>
        object Unproxy(object maybeProxy);
    }

    /// <summary>
    /// The main runtime interface between a .NET application and NHibernate. This is the central
    /// API class abstracting the notion of a persistence service.
    /// </summary>
    public interface IDbStore<T>
    {
        IQueryable<T> Query();
        /// <summary>
        /// Query entities in local cache
        /// </summary>
        IQueryable<T> QueryLocal();

        /// <summary>
        /// Return the persistent instance of the given entity class with the given identifier,
        /// obtaining the specified lock mode.
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">A valid identifier of an existing persistent instance of the class</param>
        /// <returns>the persistent instance</returns>
        T Load(object id);

        /// <summary>
        /// Return the persistent instance of the given entity type with the given identifier,
        /// or null if there is no such persistent instance. (If the instance, or a proxy for the
        /// instance, is already associated with the session, return that instance or proxy.)
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">an identifier</param>
        /// <returns>a persistent instance or null</returns>
        T Get(object id);

        /// <summary>
        /// Returns entity from ISession PersistenceContext (not yet persisted), if null it returns Get<T>(id)
        /// </summary>
        /// <typeparam name="T">A persistent class</typeparam>
        /// <param name="id">an identifier</param>
        /// <returns>a persistent instance or null</returns>
        T Find(object id);

        /// <summary>
        /// Either <c>Save()</c> or <c>Update()</c> the given instance, depending upon the value of
        /// its identifier property.
        /// </summary>
        /// <remarks>
        /// By default the instance is always saved. This behaviour may be adjusted by specifying
        /// an <c>unsaved-value</c> attribute of the identifier property mapping
        /// </remarks>
        /// <param name="model">A transient instance containing new or updated state</param>
        void Save(IEntity model);

        /// <summary>
        /// Remove a persistent instance from the datastore.
        /// </summary>
        /// <remarks>
        /// The argument may be an instance associated with the receiving <c>ISession</c> or a
        /// transient instance with an identifier associated with existing persistent state.
        /// </remarks>
        /// <param name="model">The instance to be removed</param>
        void Delete(IEntity model);

        /// <summary>
        /// Re-read the state of the given instance from the underlying database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is inadvisable to use this to implement long-running sessions that span many
        /// business tasks. This method is, however, useful in certain special circumstances.
        /// </para>
        /// <para>
        /// For example,
        /// <list>
        ///		<item>Where a database trigger alters the object state upon insert or update</item>
        ///		<item>After executing direct SQL (eg. a mass update) in the same session</item>
        ///		<item>After inserting a <c>Blob</c> or <c>Clob</c></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="model">A persistent instance</param>
        void Refresh(IEntity model);

        /// <summary>
        /// Get the entity instance underlying the given proxy, throwing
        /// an exception if the proxy is uninitialized. If the given object
        /// is not a proxy, simply return the argument.
        /// </summary>
        T Unproxy(object maybeProxy);
    }
}
