using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Linq.Functions;

namespace CoreSharp.NHibernate.Generators
{
    public class CoreSharpLinqToHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public CoreSharpLinqToHqlGeneratorsRegistry()
        {
            this.Merge(new DateTimeAddGenerator());
        }
    }
}
