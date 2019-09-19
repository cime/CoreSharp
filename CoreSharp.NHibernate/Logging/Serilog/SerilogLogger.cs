using System;
using System.Collections.Generic;
using NHibernate;
using Serilog;
using Serilog.Events;

namespace CoreSharp.NHibernate.Logging.Serilog
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="INHibernateLogger"/> interface to allow the usage
    /// of Serilog with the NHibernate logging infrastructure.
    /// </summary>
    /// <seealso cref="SerilogLoggerFactory"/>
    public class SerilogLogger : INHibernateLogger
    {
        /// <summary>
        /// Mapping between NHibernate log levels and Serilog
        /// In Serilog, non does not exists
        /// </summary>
        private static readonly Dictionary<NHibernateLogLevel, LogEventLevel> MapLevels = new Dictionary<NHibernateLogLevel, LogEventLevel>
        {
            { NHibernateLogLevel.Trace, LogEventLevel.Verbose },
            { NHibernateLogLevel.Info, LogEventLevel.Information },
            { NHibernateLogLevel.Debug, LogEventLevel.Debug },
            { NHibernateLogLevel.Warn, LogEventLevel.Warning },
            { NHibernateLogLevel.Error, LogEventLevel.Error },
            { NHibernateLogLevel.Fatal, LogEventLevel.Fatal }
        };

        private readonly ILogger _logger;

        public SerilogLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log a new entry in serilog
        /// </summary>
        /// <param name="logLevel">NHibernate log level</param>
        /// <param name="state">Log state</param>
        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
            _logger.Write(MapLevels[logLevel], exception, state.Format, state.Args);
        }

        /// <summary>
        /// Is the logging level enabled?
        /// </summary>
        /// <param name="logLevel">NHibernate log level</param>
        /// <returns>true/false</returns>
        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            return logLevel == NHibernateLogLevel.None || _logger.IsEnabled(MapLevels[logLevel]);
        }
    }
}
