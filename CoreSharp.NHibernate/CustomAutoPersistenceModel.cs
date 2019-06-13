using System;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.NHibernate.Visitors;
using FluentNHibernate.Automapping;
using FluentNHibernate.MappingModel;
using SimpleInjector;

namespace CoreSharp.NHibernate
{
    public class CustomAutoPersistenceModel : AutoPersistenceModel
    {
        private readonly Container _container;

        public CustomAutoPersistenceModel(Container container, IAutomappingConfiguration cfg) : base(cfg)
        {
            _container = container;
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

            return mappings;
        }
    }
}
