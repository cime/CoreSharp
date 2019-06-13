using CoreSharp.Breeze.Metadata;
using CoreSharp.Common.Events;
using NHibernate;

namespace CoreSharp.Breeze.Events
{
    public class BreezeMetadataBuiltEvent : IEvent
    {
        public BreezeMetadataBuiltEvent(MetadataSchema metadata, ISessionFactory sessionFactory)
        {
            Metadata = metadata;
            SessionFactory = sessionFactory;
        }

        public MetadataSchema Metadata { get; }

        public ISessionFactory SessionFactory { get; }
    }
}
