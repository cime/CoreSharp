using Microsoft.Extensions.Logging;
using NHibernate;

namespace CoreSharp.NHibernate.Logging.Microsoft
{
    /// <summary>
    /// Implementation of the <see cref="INHibernateLoggerFactory"/> interface
    /// to allow the usage of Microsoft.Extensions.Logging with the NHibernate
    /// logging infrastructure.
    /// <seealso cref="MicrosoftLogger"/>
    /// <example>
    /// To use this logger factory with NHibernate add the following code to your code:
    /// <code>
    /// loggerFactory.UseAsNHibernateLoggerProvider();
    /// </code>
    /// </example>
    /// </summary>
    public class MicrosoftLoggerFactory : INHibernateLoggerFactory
    {
        private readonly global::Microsoft.Extensions.Logging.ILoggerFactory _factory;

        public MicrosoftLoggerFactory(global::Microsoft.Extensions.Logging.ILoggerFactory factory)
        {
            _factory = factory;
        }

        #region ILoggerFactory

        public INHibernateLogger LoggerFor(string keyName)
        {
            return new MicrosoftLogger(_factory.CreateLogger(keyName));
        }

        public INHibernateLogger LoggerFor(System.Type type)
        {
            return new MicrosoftLogger(_factory.CreateLogger(type));
        }

        #endregion
    }
}
