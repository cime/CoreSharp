using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Linq.Functions;

namespace CoreSharp.NHibernate.Generators
{
    public class CoreSharpLinqToHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public CoreSharpLinqToHqlGeneratorsRegistry()
        {
            this.Merge(new AddSecondsGenerator());
            this.Merge(new AddMinutesGenerator());
            this.Merge(new AddHoursGenerator());
            this.Merge(new AddDaysGenerator());
            this.Merge(new AddMonthsGenerator());
            this.Merge(new AddYearsGenerator());
        }
    }
}
