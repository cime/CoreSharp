using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CoreSharp.NHibernate.Profiler.Contracts;
using NHibernate;

namespace CoreSharp.NHibernate.Profiler
{
    public abstract class AbstractLogger : INHibernateLogger
    {
        private static readonly IList<Assembly> IgnoredAssemblies = new[] { typeof(INHibernateLogger).Assembly, typeof(AbstractLogger).Assembly };
        private static readonly SessionIdLoggingContext SessionIdLoggingContext = new SessionIdLoggingContext();

        public static string SessionId
        {
            get
            {
                var getSessionId = SessionIdLoggingContext.GetSessionId;
                if (getSessionId != null)
                {
                    var sessionId = getSessionId();
                    if (sessionId != null)
                    {
                        return sessionId.Value.ToString();
                    }
                }

                return null;
            }
        }

        private readonly string _type;
        private static readonly Type StackFrameHelperType = typeof(object).Assembly.GetType("System.Diagnostics.StackFrameHelper");

        protected static readonly Func<List<NHibernateStackFrame>> GetStackTrace;

        //protected HttpContext HttpContext => HttpContext.Current;
        //protected HttpRequest HttpRequest => HttpContext?.Request;

        static AbstractLogger()
        {
            var getStackFramesInternalMethod = Type.GetType("System.Diagnostics.StackTrace, mscorlib").GetMethod("GetStackFramesInternal", BindingFlags.Static | BindingFlags.NonPublic);
            var p1 = Expression.Parameter(typeof(object), "p1");
            var p2 = Expression.Parameter(typeof(Exception), "p2");
            var helperInstanceExpression = Expression.New(StackFrameHelperType.GetConstructors()[0], Expression.Convert(Expression.Constant(null), typeof(Thread)));
            var internalFramesExpression = Expression.Call(getStackFramesInternalMethod, Expression.Convert(p1, StackFrameHelperType), Expression.Constant(0), Expression.Constant(true), p2);

            var createHelperInstance = Expression.Lambda<Func<object>>(Expression.Convert(helperInstanceExpression, typeof(object))).Compile();
            var getStackFramesInternal = Expression.Lambda<Action<object, Exception>>(internalFramesExpression, p1, p2).Compile();

            var helperInstance = createHelperInstance();
            getStackFramesInternal(helperInstance, null);

            var gmb = StackFrameHelperType.GetMethod("GetMethodBase");
            var gfn = StackFrameHelperType.GetMethod("GetFilename");
            var gln = StackFrameHelperType.GetMethod("GetLineNumber");
            var gcn = StackFrameHelperType.GetMethod("GetColumnNumber");
            var gfc = StackFrameHelperType.GetField("iFrameCount", BindingFlags.Instance | BindingFlags.NonPublic);
            var p3 = Expression.Parameter(typeof(int), "p3");
            var getMethodBase = Expression.Lambda<Func<object, int, MethodBase>>(Expression.Call(Expression.Convert(p1, StackFrameHelperType), gmb, p3), p1, p3).Compile();
            var getFilename = Expression.Lambda<Func<object, int, string>>(Expression.Call(Expression.Convert(p1, StackFrameHelperType), gfn, p3), p1, p3).Compile();
            var getLineNumber = Expression.Lambda<Func<object, int, int>>(Expression.Call(Expression.Convert(p1, StackFrameHelperType), gln, p3), p1, p3).Compile();
            var getColumnNumber = Expression.Lambda<Func<object, int, int>>(Expression.Call(Expression.Convert(p1, StackFrameHelperType), gcn, p3), p1, p3).Compile();
            var getFrameNumber = Expression.Lambda<Func<object, int>>(Expression.Field(Expression.Convert(p1, StackFrameHelperType), gfc), p1).Compile();


            GetStackTrace = () =>
            {
                var fn = getFrameNumber(helperInstance);

                var lines = new List<NHibernateStackFrame>(fn);

                for (var i = 0; i < fn; i++)
                {
                    var methodBase = getMethodBase(helperInstance, i);

                    //if (methodBase.DeclaringType != null && IgnoredAssemblies.Contains(methodBase.DeclaringType.Assembly))
                    //{
                    //    continue;
                    //}

                    var fileName = getFilename(helperInstance, i);
                    var lineNumber = fileName != null ? getLineNumber(helperInstance, i) : 0;
                    var columnNumber = fileName != null ? getColumnNumber(helperInstance, i) : 0;

                    lines.Add(new NHibernateStackFrame()
                    {
                        Type = methodBase.DeclaringType != null ? $"{methodBase.DeclaringType.Namespace}.{methodBase.DeclaringType.Name}" : "",
                        Method = methodBase.Name,
                        Filename = fileName,
                        Line = lineNumber,
                        Column = columnNumber
                    });
                }

                return lines;
            };
        }

        protected AbstractLogger(string type)
        {
            _type = type;
        }

        protected void LogMessage(string content, string format, object[] args)
        {
            NHibernateTcpServer.GetInstance().AddMessageToQueue(new Message()
            {
                DateTime = DateTime.UtcNow,
                Type = _type,
                Content = content,
                Arguments = args,
                Format = format,
                //Url = HttpRequest?.Url.ToString(),
                SessionId = AbstractLogger.SessionId,
                Frames = GetStackTrace()
            });
        }

        public abstract void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception);

        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            return true;
        }
    }
}
