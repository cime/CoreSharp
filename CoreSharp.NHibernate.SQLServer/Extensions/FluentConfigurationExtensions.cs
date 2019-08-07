using CoreSharp.NHibernate.Conventions.Mssql;
using FluentNHibernate.Cfg;

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
    }
}
