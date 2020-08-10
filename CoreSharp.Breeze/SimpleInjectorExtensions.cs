using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Breeze.NHibernate;
using Breeze.NHibernate.Configuration;
using Breeze.NHibernate.Serialization;
using CoreSharp.Breeze.Serialization;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using Humanizer;
using NHibernate;
using SimpleInjector;

namespace CoreSharp.Breeze
{
    public static class SimpleInjectorExtensions
    {
        private static readonly HashSet<string> VersionedEntityProperties = new HashSet<string>
        {
            nameof(IVersionedEntity.CreatedDate),
            nameof(IVersionedEntity.LastModifiedDate)
        };

        public static void AddBreeze(this Container container, Action<BreezeOptions> action = null)
        {
            var options = new BreezeOptions();
            var registeredServices = new HashSet<Type>(container.GetCurrentRegistrations().Select(o => o.ServiceType));

            container.TryRegisterSingleton<ISyntheticPropertyNameConvention, DefaultSyntheticPropertyNameConvention>(registeredServices);
            container.TryRegisterSingleton<INHibernateClassMetadataProvider, DefaultNHibernateClassMetadataProvider>(registeredServices);
            container.TryRegisterSingleton<IPropertyValidatorsProvider, FluentValidationPropertyValidatorsProvider>(registeredServices);
            container.TryRegisterSingleton<IJsonSerializerSettingsProvider, BreezeJsonSerializerSettingsProvider>(registeredServices);
            container.TryRegisterSingleton<IEntityQueryExecutor, DefaultEntityQueryExecutor>(registeredServices);
            container.TryRegisterSingleton<ILazyLoadGuardProvider, DefaultLazyLoadGuardProvider>(registeredServices);
            container.TryRegisterSingleton<IEntityMetadataProvider, EntityMetadataProvider>(registeredServices);
            container.TryRegisterSingleton<IClientModelMetadataProvider, ClientModelMetadataProvider>(registeredServices);
            container.TryRegisterSingleton<IProxyInitializer, ProxyInitializer>(registeredServices);
            container.TryRegisterSingleton<IEntityBatchFetcherFactory, EntityBatchFetcherFactory>(registeredServices);
            container.TryRegisterSingleton<IModelSaveValidatorProvider, DefaultModelSaveValidatorProvider>(registeredServices);
            container.TryRegisterSingleton<ITypeMembersProvider, DefaultTypeMembersProvider>(registeredServices);
            container.TryRegisterSingleton<ISaveWorkStateFactory, SaveWorkStateFactory>(registeredServices);
            container.TryRegisterSingleton<BreezeContractResolver>(registeredServices);
            container.TryRegisterSingleton<IBreezeConfigurator>(() => {
                var configurator = new BreezeConfigurator(container.GetInstance<ITypeMembersProvider>());
                Configure(configurator);
                options.BreezeConfigurator?.Invoke(configurator);

                return configurator;
            }, registeredServices);
            container.TryRegisterSingleton(() => {
                var builder = container.GetInstance<BreezeMetadataBuilder>()
                    .WithOrphanDeleteEnabled()
                    .WithPluralizeFunction(name => name.Pluralize());
                options.MetadataConfigurator?.Invoke(builder);

                return builder.Build();
            }, registeredServices);

            container.TryRegisterScoped<EntityUpdater>(registeredServices);
            container.TryRegisterScoped<PersistenceManager>(registeredServices);
            container.TryRegisterScoped<ISessionProvider>(() => new DefaultSessionProvider(type => container.GetInstance<ISession>()), registeredServices);

            container.TryRegisterTransient<BreezeMetadataBuilder>(registeredServices);

            action?.Invoke(options);
        }

        private static void Configure(IBreezeConfigurator configurator)
        {
            configurator.ConfigureModelMembers(o => typeof(IClientModel).IsAssignableFrom(o),
                (member, o) =>
                {
                    if (member.GetCustomAttribute<NotNullAttribute>() != null)
                    {
                        o.IsNullable(false);
                    }
                });
            configurator.ConfigureModelMembers(o => typeof(IVersionedEntity).IsAssignableFrom(o),
                (member, o) =>
                {
                    if (VersionedEntityProperties.Contains(member.Name))
                    {
                        o.IsNullable(true);
                        o.Deserialize(false);
                    }
                });
        }

        private static void TryRegisterSingleton<TService, TImplementation>(this Container container, HashSet<Type> registeredServices)
            where TService : class where TImplementation : class, TService
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.RegisterSingleton<TService, TImplementation>();
        }

        private static void TryRegisterSingleton<TService>(this Container container, HashSet<Type> registeredServices)
            where TService : class
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.RegisterSingleton<TService>();
        }

        private static void TryRegisterSingleton<TService>(this Container container, Func<TService> instanceCreator, HashSet<Type> registeredServices)
            where TService : class
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.RegisterSingleton(instanceCreator);
        }

        private static void TryRegisterScoped<TService>(this Container container, HashSet<Type> registeredServices)
            where TService : class
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.Register<TService>(Lifestyle.Scoped);
        }

        private static void TryRegisterScoped<TService>(this Container container, Func<TService> instanceCreator, HashSet<Type> registeredServices)
            where TService : class
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.Register(instanceCreator, Lifestyle.Scoped);
        }

        private static void TryRegisterTransient<TService>(this Container container, HashSet<Type> registeredServices)
            where TService : class
        {
            if (registeredServices.Contains(typeof(TService)))
            {
                return;
            }

            container.Register<TService>(Lifestyle.Transient);
        }
    }
}
