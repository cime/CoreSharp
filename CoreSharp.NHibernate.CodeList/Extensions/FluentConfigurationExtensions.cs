using CoreSharp.NHibernate.CodeList.Conventions;
using FluentNHibernate.Cfg;

namespace CoreSharp.NHibernate.CodeList.Extensions
{
    public static class FluentConfigurationExtensions
    {
        public static FluentConfiguration AddCodeListConventions(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.AddFromAssemblyOf<FilterCurrentLanguageAttributeConvention>();
                }
            });
        }
    }
}
