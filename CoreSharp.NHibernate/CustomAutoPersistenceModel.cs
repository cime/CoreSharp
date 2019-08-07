using System;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Events;
using CoreSharp.NHibernate.Events;
using CoreSharp.NHibernate.Visitors;
using FluentNHibernate.Automapping;
using FluentNHibernate.MappingModel;
using SimpleInjector;

namespace CoreSharp.NHibernate
{
    public class CustomAutoPersistenceModel : AutoPersistenceModel
    {
        private readonly Container _container;
        private readonly IEventPublisher _eventPublisher;
        private readonly global::NHibernate.Cfg.Configuration _configuration;

        public CustomAutoPersistenceModel(
            Container container,
            IAutomappingConfiguration cfg,
            IEventPublisher eventPublisher,
            global::NHibernate.Cfg.Configuration configuration) : base(cfg)
        {
            _container = container;
            _eventPublisher = eventPublisher;
            _configuration = configuration;
        }

        public override IEnumerable<HibernateMapping> BuildMappings()
        {
            var mappings =  base.BuildMappings().ToList();

            try
            {
                var visitors = _container.GetAllInstances<INHibernateMappingVisitor>();

                foreach (var visitor in visitors)
                {
                    visitor.Visit(mappings);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _eventPublisher.Publish(new MappingsBuiltEvent(_configuration, mappings));

            return mappings;
        }
    }
}
