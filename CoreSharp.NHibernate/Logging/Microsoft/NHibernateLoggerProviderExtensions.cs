using NHibernate;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace CoreSharp.NHibernate.Logging.Microsoft
{
    public static class NHibernateLoggerProviderExtensions
    {
        public static ILoggerFactory UseAsNHibernateLoggerProvider(this ILoggerFactory factory)
        {
            NHibernateLogger.SetLoggersFactory(new MicrosoftLoggerFactory(factory));
            return factory;
        }
    }
}
