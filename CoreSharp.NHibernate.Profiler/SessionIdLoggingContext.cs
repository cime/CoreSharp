using System;
using System.Reflection;

namespace CoreSharp.NHibernate.Profiler
{
    public class SessionIdLoggingContext
    {
        public SessionIdLoggingContext()
        {
            var type = Type.GetType("NHibernate.Impl.SessionIdLoggingContext, NHibernate");
            if (type == (Type) null)
            {
                return;
            }

            var property = type.GetProperty("SessionId", BindingFlags.Static | BindingFlags.Public);

            if (property == (PropertyInfo) null)
            {
                return;
            }

            var getMethod = property.GetGetMethod();

            if (getMethod == (MethodInfo) null)
            {
                return;
            }

            GetSessionId = (SessionIdLoggingContext.GetSessionIdDelegate)Delegate.CreateDelegate(typeof(SessionIdLoggingContext.GetSessionIdDelegate), getMethod);
        }

        public SessionIdLoggingContext.GetSessionIdDelegate GetSessionId { get; private set; }

        public delegate Guid? GetSessionIdDelegate();
    }
}
