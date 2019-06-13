using CoreSharp.DataAccess;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            var isCodeList = typeof(ICodeList).IsAssignableFrom(instance.EntityType);
            instance.Column(isCodeList ? "Code" : "Id");
        }
    }
}
