using System.Collections.Generic;
using CoreSharp.Cqrs.Events;
using FluentNHibernate.MappingModel;

namespace CoreSharp.NHibernate.Events
{
    public class MappingsBuiltEvent : IEvent
    {
        public MappingsBuiltEvent(global::NHibernate.Cfg.Configuration configuration, IEnumerable<HibernateMapping> mappings)
        {
            Configuration = configuration;
            Mappings = mappings;
        }

        public IEnumerable<HibernateMapping> Mappings { get; }

        public global::NHibernate.Cfg.Configuration Configuration { get; set; }
    }
}
