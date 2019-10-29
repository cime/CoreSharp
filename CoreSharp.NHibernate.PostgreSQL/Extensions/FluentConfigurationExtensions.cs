using System;
using System.Linq;
using System.Reflection;
using CoreSharp.NHibernate.PostgreSQL.Conventions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions;

namespace CoreSharp.NHibernate.PostgreSQL.Extensions
{
    public static class FluentConfigurationExtensions
    {
        public static FluentConfiguration CreatePostgreSQLHiLoSchema(this FluentConfiguration fluentConfiguration, bool create = true)
        {
            if (!create)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                PostgresqlHiLoIdConvention.SchemaCreate(cfg);
            });
        }

        public static FluentConfiguration AddPostgreSQLConventions(this FluentConfiguration fluentConfiguration)
        {
            var cfg = (global::NHibernate.Cfg.Configuration)fluentConfiguration.GetMemberValue("cfg");
            var namingStrategy = cfg.NamingStrategy;
            var conventions = typeof(PostgresqlHiLoIdConvention)
                .Assembly.GetTypes()
                .Where(x => typeof(IConvention).IsAssignableFrom(x))
                .OrderByDescending(x => x.GetPriority())
                .ToList();

            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.Add(typeof(DateTimeOffsetPropertyConvention));
                    persistenceModel.Conventions.Add(typeof(DefaultValueAttributeConvention));
                    persistenceModel.Conventions.Add(typeof(PostgresqlHiLoIdConvention), new PostgresqlHiLoIdConvention(cfg));
                }
            });
        }
    }
}
