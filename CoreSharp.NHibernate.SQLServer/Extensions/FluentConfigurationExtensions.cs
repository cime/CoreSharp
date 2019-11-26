using CoreSharp.NHibernate.Conventions.Mssql;
using CoreSharp.NHibernate.Extensions;
using FluentNHibernate.Cfg;
using System.Linq;

namespace CoreSharp.NHibernate.SQLServer.Extensions
{
    public static class FluentConfigurationExtensions
    {
        public static FluentConfiguration CreateSQLServerHiLoSchema(this FluentConfiguration fluentConfiguration, bool create = true)
        {
            if (!create)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                MssqlHiLoIdConvention.SchemaCreate(cfg);
            });
        }

        public static FluentConfiguration AddSQLServerConventions(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.AddFromAssemblyOf<MssqlHiLoIdConvention>();
                }
            });
        }

        public static FluentConfiguration CreateSQLServerSchemas(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.ExposeDbCommand((command, cfg) =>
            {
                var schemas = cfg.ClassMappings.Select(x => x.Table.Schema).Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .Select(x => cfg.NamingStrategy.TableName(x))
                    .ToList();

                foreach (var schema in schemas)
                {
                    command.CommandText = $"IF NOT EXISTS(SELECT * FROM sys.schemas  WHERE name = N'{schema}') EXEC('CREATE SCHEMA [{schema}]');";
                    command.ExecuteNonQuery();
                }
            });
        }
    }
}
