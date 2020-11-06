using CoreSharp.NHibernate.Extensions;
using FluentNHibernate.Cfg;
using System.Linq;
using System.Reflection;
using CoreSharp.NHibernate.Generators;
using CoreSharp.NHibernate.SQLServer.Conventions;
using NHibernate;
using NHibernate.Dialect.Function;

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
            var cfg = (global::NHibernate.Cfg.Configuration)fluentConfiguration.GetMemberValue("cfg");

            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.AddFromAssemblyOf<MssqlHiLoIdConvention>();
                    persistenceModel.Conventions.Add(typeof(ForeignKeyNameConvention));
                    persistenceModel.Conventions.Add(typeof(FormulaAttributeBooleanConvention));
                    persistenceModel.Conventions.Add(typeof(MssqlHiLoIdConvention), new MssqlHiLoIdConvention(cfg));
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
        
        public static FluentConfiguration RegisterDateTimeGenerators(this FluentConfiguration fluentConfiguration)
        {
            fluentConfiguration.ExposeConfiguration(config =>
            {
                config.AddSqlFunction("AddSeconds", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(second, ?2, ?1)"));
                config.AddSqlFunction("AddMinutes", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(minute, ?2, ?1)"));
                config.AddSqlFunction("AddHours", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(hour, ?2, ?1)"));
                config.AddSqlFunction("AddDays", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(day, ?2, ?1)"));
                config.AddSqlFunction("AddMonths", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(month, ?2, ?1)"));
                config.AddSqlFunction("AddYears", new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATEADD(year, ?2, ?1)"));

                config.LinqToHqlGeneratorsRegistry<CoreSharpLinqToHqlGeneratorsRegistry>();
            });

            return fluentConfiguration;
        }
    }
}
