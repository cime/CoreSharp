using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Events;
using CoreSharp.NHibernate.Events;
using CoreSharp.NHibernate.Visitors;
using FluentNHibernate.Automapping;
using FluentNHibernate.MappingModel;
using FluentNHibernate.Utils;
using FluentNHibernate.Utils.Reflection;
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

        private void AddOverride(Type type, Action<object> action)
        {
            var field = typeof(AutoPersistenceModel).GetField("inlineOverrides",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var inlineOverrides = (List<InlineOverride>)field.GetValue(this);

            inlineOverrides.Add(new InlineOverride(type, action));
        }

        private static bool IsAutomappingForType(object o, Type entityType)
        {
            var autoMappingType = ReflectionHelper.AutomappingTypeForEntityType(entityType);

            return o.GetType().IsAssignableFrom(autoMappingType);
        }

        /// <summary>
        /// Adds an IAutoMappingOverride reflectively
        /// </summary>
        /// <param name="overrideType">Override type, expected to be an IAutoMappingOverride</param>
        public new void Override(Type overrideType)
        {
            var overrideInterfaces = overrideType.GetInterfaces().Where(x => x.IsAutoMappingOverrideType()).ToList();

            foreach (var overrideInterface in overrideInterfaces)
            {
                var entityType = overrideInterface.GetGenericArguments().First();

                AddOverride(entityType, instance =>
                {
                    if (!IsAutomappingForType(instance, entityType)) return;

                    var overrideInstance = _container.GetInstance(overrideInterface);

                    var overrideHelperMethod = typeof(AutoPersistenceModel)
                        .GetMethod("OverrideHelper", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (overrideHelperMethod == null) return;

                    overrideHelperMethod
                        .MakeGenericMethod(entityType)
                        .Invoke(this, new[] { instance, overrideInstance });
                });
            }
        }
    }
}
