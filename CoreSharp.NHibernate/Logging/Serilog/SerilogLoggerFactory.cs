using Microsoft.Extensions.Logging;
using NHibernate;
using Serilog;
using Serilog.Core;

namespace CoreSharp.NHibernate.Logging.Serilog
{
    /// <summary>
    /// Implementation of the <see cref="INHibernateLoggerFactory"/> interface
    /// to allow the usage of Serilog with the NHibernate
    /// logging infrastructure.
    /// <seealso cref="SerilogLogger"/>
    /// <example>
    /// To use this logger factory with NHibernate add the following code to your code:
    /// <code>
    /// loggerFactory.UseAsNHibernateLoggerProvider();
    /// </code>
    /// </example>
    /// </summary>
    public class SerilogLoggerFactory : INHibernateLoggerFactory
    {
        public INHibernateLogger LoggerFor(string keyName)
        {
            var contextLogger = Log.Logger.ForContext(Constants.SourceContextPropertyName, keyName);
            return new SerilogLogger(contextLogger);
        }

        public INHibernateLogger LoggerFor(System.Type type)
        {
            var contextLogger = Log.Logger.ForContext(type);
            return new SerilogLogger(contextLogger);
        }
    }
}
