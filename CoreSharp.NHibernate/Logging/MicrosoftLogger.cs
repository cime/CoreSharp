using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace CoreSharp.NHibernate.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="INHibernateLogger"/> interface to allow the usage
    /// of Microsoft.Extensions.Logging with the NHibernate logging infrastructure.
    /// </summary>
    /// <seealso cref="MicrosoftLoggerFactory"/>
    public class MicrosoftLogger : INHibernateLogger
    {
        private static readonly IDictionary<NHibernateLogLevel, LogLevel> _logLevelFor = new Dictionary<NHibernateLogLevel, LogLevel>
        {
            [NHibernateLogLevel.Trace] = LogLevel.Trace,
            [NHibernateLogLevel.Debug] = LogLevel.Debug,
            [NHibernateLogLevel.Info] = LogLevel.Information,
            [NHibernateLogLevel.Warn] = LogLevel.Warning,
            [NHibernateLogLevel.Error] = LogLevel.Error,
            [NHibernateLogLevel.Fatal] = LogLevel.Critical,
            [NHibernateLogLevel.None] = LogLevel.None
        };
        private readonly ILogger _logger;

        public MicrosoftLogger(ILogger logger)
        {
            _logger = logger;
        }

        #region INHibernateLogger
        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
            _logger.Log(logLevel: _logLevelFor[logLevel],
                eventId: 0,
                state: state,
                exception: exception,
                formatter: (s,e) => s.ToString());
        }

        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            return _logger.IsEnabled(_logLevelFor[logLevel]);
        }
        #endregion
    }
}
