using CoreSharp.Cqrs.Events;
using NHibernate;

namespace CoreSharp.NHibernate.Events
{
    public class SessionFlushedEvent : IAsyncEvent
    {
        public ISession Session { get; }

        public SessionFlushedEvent(ISession session)
        {
            Session = session;
        }
    }
}
