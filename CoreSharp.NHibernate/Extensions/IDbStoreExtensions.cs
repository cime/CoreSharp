using System.Reflection;
using CoreSharp.DataAccess;
using NHibernate;

namespace CoreSharp.NHibernate.Extensions
{
    public static class IDbStoreExtensions
    {
        public static T LoadIf<T>(this IDbStore dbStore, object id)
            where T : IEntity
        {
            return id == null ? default(T) : dbStore.Load<T>(id);
        }

        public static T GetIf<T>(this IDbStore dbStore, object id)
            where T : IEntity
        {
            return id == null ? default(T) : dbStore.Get<T>(id);
        }

        public static ISession GetSession(this IDbStore dbStore)
        {
            return dbStore.GetMemberValue("_session") as ISession;
        }
    }
}
