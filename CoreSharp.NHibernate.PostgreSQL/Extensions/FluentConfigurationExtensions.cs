using System.Linq;
using System.Reflection;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Extensions;
using CoreSharp.NHibernate.Generators;
using CoreSharp.NHibernate.PostgreSQL.Conventions;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect.Function;
using NHibernate.Mapping;

namespace CoreSharp.NHibernate.PostgreSQL.Extensions
{
    public static class FluentConfigurationExtensions
    {
        private static string GetFullName(Table table, INamingStrategy namingStrategy)
        {
            return string.Join(".", new [] { namingStrategy.TableName(table.Schema ?? "public"), namingStrategy.TableName(table.Name.TrimStart('`').TrimEnd('`')) }
                .Where(x => !string.IsNullOrEmpty(x)));
        }

        public static FluentConfiguration InsertPostgreSQLHiLoValues(this FluentConfiguration fluentConfiguration, bool execute = true)
        {
            if (!execute)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.ExposeDbCommand((command, cfg) =>
            {
                var hiLoTable = PostgreSqlHiLoIdConvention.HiLoIdentityTableName;
                var entityColumn = PostgreSqlHiLoIdConvention.TableColumnName;
                var valueColumn = PostgreSqlHiLoIdConvention.NextHiValueColumnName;
                var tables = cfg.ClassMappings.Where(x => !typeof(ICodeList).IsAssignableFrom(x.MappedClass)).Select(x => x.Table).Where(x => !string.IsNullOrEmpty(x.Name))
                    .Distinct()
                    .Select(x => GetFullName(x, cfg.NamingStrategy))
                    .ToList();

                foreach (var tableFullName in tables)
                {
                    command.CommandText = $"INSERT INTO {hiLoTable} ({entityColumn}, {valueColumn}) SELECT '{tableFullName}', 0 WHERE NOT EXISTS (SELECT 1 FROM {hiLoTable} WHERE {entityColumn} = '{tableFullName}');";
                    command.ExecuteNonQuery();
                }
            });
        }

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
                PostgreSqlHiLoIdConvention.SchemaCreate(cfg);
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
                    persistenceModel.Conventions.Add(typeof(JsonConvention));
                    persistenceModel.Conventions.Add(typeof(JsonbConvention));
                    persistenceModel.Conventions.Add(typeof(IndexAttributeConvention));
                    persistenceModel.Conventions.Add(typeof(PostgreSqlHiLoIdConvention), new PostgreSqlHiLoIdConvention(cfg));
                }
            });
        }

        public static FluentConfiguration RegisterDateTimeGenerators(this FluentConfiguration fluentConfiguration)
        {
            fluentConfiguration.ExposeConfiguration(config =>
            {
                config.AddSqlFunction("AddSeconds", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 second')"));
                config.AddSqlFunction("AddMinutes", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 minute')"));
                config.AddSqlFunction("AddHours", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 hour')"));
                config.AddSqlFunction("AddDays", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 day')"));
                config.AddSqlFunction("AddMonths", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 month')"));
                config.AddSqlFunction("AddYears", new SQLFunctionTemplate(NHibernateUtil.DateTime, "?1 + (?2 * interval '1 year')"));

                config.LinqToHqlGeneratorsRegistry<CoreSharpLinqToHqlGeneratorsRegistry>();
            });

            return fluentConfiguration;
        }
    }
}
