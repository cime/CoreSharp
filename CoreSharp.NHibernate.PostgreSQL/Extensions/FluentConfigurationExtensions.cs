using System.Linq;
using CoreSharp.NHibernate.PostgreSQL.Conventions;
using FluentNHibernate.Cfg;

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
            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.AddFromAssemblyOf<PostgresqlHiLoIdConvention>();
                }
            });
        }
    }
}
