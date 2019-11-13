using System;
using NHibernate;

namespace CoreSharp.NHibernate.Profiler
{
    public class NoLoggingNHibernateLogger : INHibernateLogger
    {
        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
        }

        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            return logLevel == NHibernateLogLevel.None;
        }
    }
}
