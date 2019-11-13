using NHibernate;

namespace CoreSharp.NHibernate.Profiler
{
    public static class NHibernateProfiler
    {
        public static readonly string[] LoggerNames = new string[17]
        {
            "NHibernate.Transaction.AdoTransaction",
            "NHibernate.SQL",
            "NHibernate.Impl.SessionImpl",
            "NHibernate.Impl.StatelessSessionImpl",
            "NHibernate.Impl.AbstractSessionImpl",
            "NHibernate.Impl.MultiQueryImpl",
            "NHibernate.Transaction.ITransactionFactory",
            "NHibernate.Event.Default.DefaultInitializeCollectionEventListener",
            "NHibernate.Event.Default.DefaultLoadEventListener",
            "NHibernate.Cache.StandardQueryCache",
            "NHibernate.Persister.Entity.AbstractEntityPersister",
            "NHibernate.Loader.Loader",
            "NHibernate.AdoNet.AbstractBatcher",
            "NHibernate.Tool.hbm2ddl.SchemaExport",
            "NHibernate.Tool.hbm2ddl.SchemaUpdate",
            "NHibernate.Search.Query.FullTextQueryImpl",
            "NHibernate.Impl.SessionFactoryImpl"
        };

        public static void Initialize()
        {
            NHibernateLogger.SetLoggersFactory(new NHibernateProfilerLoggerFactory());
        }
    }
}
