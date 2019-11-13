using System;
using NHibernate;

namespace CoreSharp.NHibernate.Profiler
{
    public class NHibernateProfilerLogger : AbstractLogger
    {
        public NHibernateProfilerLogger() : base("NHibernate.SQL")
        {
        }

        public override void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
            LogMessage(state.ToString(), state.Format, state.Args);
        }
    }
}
