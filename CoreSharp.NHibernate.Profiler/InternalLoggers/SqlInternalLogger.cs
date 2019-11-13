//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using NHibernateProfiler.Integration.Core;

//namespace NHibernate.Glimpse.InternalLoggers
//{
//    internal class SqlInternalLogger : INHibernateLogger
//    {
//        private readonly Assembly _thisAssem = typeof(SqlInternalLogger).Assembly;
//        private readonly Assembly _nhAssem = typeof(INHibernateLogger).Assembly;
        
//        public void Debug(object message)
//        {
//            if (message == null) return;
//            if (!LoggerFactory.LogRequest()) return;
//            var stackFrames = new System.Diagnostics.StackTrace().GetFrames();
//            var methods = new List<MethodBase>();
//            if (stackFrames != null)
//            {
//                foreach (var frame in stackFrames)
//                {
//                    var meth = frame.GetMethod();
//                    var type = meth.DeclaringType;
//                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
//                    //this can happen for emitted types
//                    if (type != null)
//                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
//                    {
//                        var assem = type.Assembly;
//                        if (Equals(assem, _thisAssem)) continue;
//                        if (Equals(assem, _nhAssem)) continue; 
//                    }
//                    methods.Add(frame.GetMethod());
//                }
//            }
//            // ReSharper disable ConditionIsAlwaysTrueOrFalse
//            var frames = methods
//                .Select(method => string.Format("{0} -> {1}", (method.DeclaringType == null) ? "DYNAMIC" : method.DeclaringType.ToString(), method))
//                .ToList();
//            // ReSharper restore ConditionIsAlwaysTrueOrFalse
//            var item = new LogStatistic(null, null)
//                           {
//                               Sql = message.ToString(),
//                               StackFrames = frames,
//                               ExecutionType = (methods.Count == 0)
//                                                   ? null
//                                                   : (methods[0].DeclaringType == null)
//                                                         ? "Object"
//                                                         : methods[0].DeclaringType.Name,
//                               ExecutionMethod = (methods.Count == 0) ? null : methods[0].Name,
//                           };
//            SqlCommandExecuted(item);
//            Log(item);
//        }
        
//        static void SqlCommandExecuted(LogStatistic logStatistic)
//        {
//            if (_timerStrategy == null) return;
//            var timer = _timerStrategy.Invoke();
//            if (timer == null) return;
//            var point = timer.Point();

//            var pointTimelineMessage = new NHibernateTimelineMessage
//                                           {
//                                               Duration = point.Duration,
//                                               Offset = point.Offset,
//                                               StartTime = point.StartTime,
//                                               EventName = string.Format("{0}:{1}", logStatistic.ExecutionType, logStatistic.ExecutionMethod),
//                                               EventSubText = logStatistic.Id.ToString()
//                                           };
//          _messageBroker.Publish(pointTimelineMessage);
//        }
        
//        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
//        {
//            throw new NotImplementedException();
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