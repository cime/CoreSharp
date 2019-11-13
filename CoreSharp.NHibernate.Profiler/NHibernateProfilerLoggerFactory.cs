using System;
using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Connection;

namespace CoreSharp.NHibernate.Profiler
{
    public class NHibernateProfilerLoggerFactory : INHibernateLoggerFactory
    {
        public INHibernateLogger LoggerFor(string keyName)
        {
            if (keyName?.ToLower().Trim() == "nhibernate.sql")
            {
                return new SqlLogger();

            }

            return new NoLoggingNHibernateLogger();
        }

        public INHibernateLogger LoggerFor(Type type)
        {
            return LoggerFor($"{type.Namespace}.{type.Name}");
        }

        public INHibernateLogger GetLogger(Type type)
        {
            if (type is AbstractBatcher)
            {

            }
            else if (type is ConnectionProvider)
            {

            }

            return new NoLoggingNHibernateLogger();
        }
    }
}
