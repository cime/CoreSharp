using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSharp.Common.Tests;
using CoreSharp.Cqrs.Events;
using CoreSharp.NHibernate;
using CoreSharp.NHibernate.Configuration;
using CoreSharp.NHibernate.Conventions;
using CoreSharp.NHibernate.Extensions;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SimpleInjector;

namespace CoreSharp.Breeze.Tests
{
    // TODO: Move into a new common test project for db testing
    public abstract class BaseDatabaseTest : BaseTest
    {
        protected BaseDatabaseTest(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        protected ISessionFactory SessionFactory => Container.GetInstance<ISessionFactory>();

        protected abstract IEnumerable<Assembly> GetEntityAssemblies();

        protected override void ConfigureContainer(Container container)
        {
            container.Register(() => CreateNHibernateConfiguration(container), Lifestyle.Singleton);
            container.Register(() => container.GetInstance<Configuration>().BuildSessionFactory(), Lifestyle.Singleton);
            container.Register(() =>
            {
                var session = container.GetInstance<ISessionFactory>().OpenSession();
                session.FlushMode = FlushMode.Commit;

                return session;
            }, Lifestyle.Scoped);
            container.Register(() => container.GetInstance<ISessionFactory>().OpenStatelessSession(), Lifestyle.Scoped);
        }

        protected override void SetUp()
        {
            var schemaExport = new SchemaExport(Container.GetInstance<Configuration>());
            schemaExport.Create(true, true);
        }

        protected override void Cleanup()
        {
            var schemaExport = new SchemaExport(Container.GetInstance<Configuration>());
            schemaExport.Drop(true, true);
        }

        protected virtual Configuration CreateNHibernateConfiguration(Container container)
        {
            var config = GetNHibernateConfiguration();
            var persistenceModel = GetPersistenceModel(container, config);
            return Fluently.Configure(config)
                .Database(
                    PostgreSQLConfiguration.PostgreSQL82.ConnectionString(o => o
                        .Database("coresharp")
                        .Host("localhost")
                        .Username("coresharp")
                        .Password("coresharp")
                        .Port(5432))
                )
                //.AppendEventListeners<User>(container)
                .SetDefaultProperties()
                .Mappings(m =>
                {
                    m.AutoMappings.Add(persistenceModel);
                })
                .ExportMappings(@"Mappings")
                .BuildConfiguration();
        }

        protected virtual Configuration GetNHibernateConfiguration()
        {
            return new Configuration();
        }

        protected virtual IAutomappingConfiguration GetAutomappingConfiguration()
        {
            return new AutomappingConfiguration();
        }

        protected virtual IEnumerable<Assembly> GetAutoMappingOverrideAssemblies()
        {
            return GetEntityAssemblies();
        }

        protected virtual CustomAutoPersistenceModel GetPersistenceModel(Container container, Configuration config)
        {
            var persistenceModel = new CustomAutoPersistenceModel(
                container,
                GetAutomappingConfiguration(),
                container.GetInstance<IEventPublisher>(),
                config);
            persistenceModel.AddTypeSource(new CombinedAssemblyTypeSource(GetEntityAssemblies()));
            persistenceModel
                .Conventions.AddFromAssemblyOf<NotNullConvention>()
                .UseOverridesFromAssemblies(GetAutoMappingOverrideAssemblies());

            return persistenceModel;
        }
    }
}
