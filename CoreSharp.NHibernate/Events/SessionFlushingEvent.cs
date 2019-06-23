using CoreSharp.Cqrs.Events;
using NHibernate;

namespace CoreSharp.NHibernate.Events
{
    public class SessionFlushingEvent : IAsyncEvent
    {
        public ISession Session { get; }

        public SessionFlushingEvent(ISession session)
        {
            Session = session;
        }
    }
}
