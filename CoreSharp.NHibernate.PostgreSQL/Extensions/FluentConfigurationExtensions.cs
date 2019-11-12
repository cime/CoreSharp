using System.Linq;
using System.Reflection;
using CoreSharp.NHibernate.Extensions;
using CoreSharp.NHibernate.PostgreSQL.Conventions;
using FluentNHibernate.Cfg;

namespace CoreSharp.NHibernate.PostgreSQL.Extensions
{
    public static class FluentConfigurationExtensions
    {
        public static FluentConfiguration CreatePostgreSQLSchemas(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.ExposeDbCommand((command, cfg) =>
            {
                var schemas = cfg.ClassMappings.Select(x => x.Table.Schema).Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .Select(x => cfg.NamingStrategy.TableName(x))
                    .ToList();

                foreach (var schema in schemas)
                {
                    command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schema};";
                    command.ExecuteNonQuery();
                }
            });
        }

        public static FluentConfiguration CreatePostgreSQLCitextExtension(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.ExposeDbCommand((command, configuration) =>
            {
                command.CommandText = "CREATE EXTENSION IF NOT EXISTS citext;";
                command.ExecuteNonQuery();
            });
        }

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

            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.Add(typeof(DateTimeOffsetPropertyConvention));
                    persistenceModel.Conventions.Add(typeof(DefaultValueAttributeConvention));
                    persistenceModel.Conventions.Add(typeof(CitextConvention));
                    persistenceModel.Conventions.Add(typeof(PostgresqlHiLoIdConvention), new PostgresqlHiLoIdConvention(cfg));
                }
            });
        }
    }
}
