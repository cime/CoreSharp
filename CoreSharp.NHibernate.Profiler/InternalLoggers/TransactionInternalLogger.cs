//using NHibernateProfiler.Integration.Core;
//using System;
//using System.Globalization;

//namespace NHibernate.Glimpse.InternalLoggers
//{
//    internal class TransactionInternalLogger : INHibernateLogger
//    {
//        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
//        {
//            var message = state.ToString();
            
//            var timestamp = DateTime.Now;
//            var item = new LogStatistic(null, null)
//            {
//                TransactionNotification =
//                    string.Format("{0}{1}", message.Trim().UppercaseFirst(),
//                        string.Format(" @ {0}.{1}.{2}.{3}",
//                            timestamp.Hour.ToString(CultureInfo.InvariantCulture)
//                                .PadLeft(2, '0'),
//                            timestamp.Minute.ToString(CultureInfo.InvariantCulture)
//                                .PadLeft(2, '0'),
//                            timestamp.Second.ToString(CultureInfo.InvariantCulture)
//                                .PadLeft(2, '0'),
//                            timestamp.Millisecond.ToString(
//                                CultureInfo.InvariantCulture).PadLeft(3, '0')))
//            };
//        }

//        public bool IsEnabled(NHibernateLogLevel logLevel)
//        {
//            if (logLevel == NHibernateLogLevel.Debug)
//            {
//                return LoggerFactory.LogRequest();
//            }

//            return false;
//        }
//    }
//}