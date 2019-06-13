using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreSharp.NHibernate.Conventions.Mssql;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using SimpleInjector;

namespace CoreSharp.NHibernate.Configuration
{
    [Obsolete]
    public class DatabaseConfiguration
    {
        private readonly Container _container;
        private readonly FluentConfiguration _fluentConfiguration;

        public virtual List<Assembly> EntityAssemblies { get; }
        public virtual List<Assembly> ConventionAssemblies { get; }

        public virtual bool GenerateStatistics { get; internal set; }
        public virtual bool ValidateSchema { get; internal set; }
        public virtual bool RecreateSchema { get; internal set; }
        public virtual bool UpdateSchema { get; internal set; }

        public virtual string HbmMappingsPath { get; internal set; }

        internal DatabaseConfiguration(SimpleInjector.Container container)
        {
            _container = container;
            _fluentConfiguration = Fluently.Configure();

            EntityAssemblies = new List<Assembly>();
            ConventionAssemblies = new List<Assembly>();

            _fluentConfiguration = Fluently.Configure().ExposeConfiguration(cfg =>
            {
                if (RecreateSchema)
                {
                    MssqlHiLoIdConvention.SchemaCreate(cfg);
                    new SchemaExport(cfg).Execute(true, true, false);
                }
                else if (UpdateSchema)
                {
                    new SchemaUpdate(cfg).Execute(true, true);
                }
            })
            .Mappings(m =>
            {
                var cfg = new AutomappingConfiguration(_container.GetInstance<IConfiguration>());
                var persistenceModel = new CustomAutoPersistenceModel(container, cfg);
                persistenceModel.AddTypeSource(new CombinedAssemblyTypeSource(EntityAssemblies.Select(a => new AssemblyTypeSource(a))));

                m.UsePersistenceModel(persistenceModel);

                foreach (var entityAssembly in EntityAssemblies)
                {
                    m.HbmMappings.AddFromAssembly(entityAssembly);
                }


                m.AutoMappings.Add(persistenceModel
                    .AddConventions(ConventionAssemblies)
                    .UseOverridesFromAssemblies(EntityAssemblies));

#if DEBUG
                if (Directory.Exists(@"C:\Temp\Baku"))
                {
                    m.AutoMappings.ExportTo(@"C:\Temp\Baku");
                }
#endif
            });
        }

        public DatabaseConfiguration Database(IPersistenceConfigurer config)
        {
            _fluentConfiguration.Database(config);

            return this;
        }

        public DatabaseConfiguration Cache(Action<CacheSettingsBuilder> cacheExpression)
        {
            _fluentConfiguration.Cache(cacheExpression);

            return this;
        }

        public static DatabaseConfiguration Configure(SimpleInjector.Container container)
        {
            return new DatabaseConfiguration(container);
        }

        public DatabaseConfiguration Recreate(bool value)
        {
            RecreateSchema = value;

            return this;
        }

        public DatabaseConfiguration Update(bool value)
        {
            UpdateSchema = value;

            return this;
        }

        public global::NHibernate.Cfg.Configuration Build()
        {
            return _fluentConfiguration.BuildConfiguration();
        }
    }
}
