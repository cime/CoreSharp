using System.Reflection;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.CodeList.Conventions
{
    public class CodeListConvention : IIdConvention, IPropertyConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            if (!typeof(ICodeList).IsAssignableFrom(instance.EntityType))
            {
                return;
            }

            var codeListConfigurationAttribute = instance.EntityType.GetCustomAttribute<CodeListConfigurationAttribute>() ?? new CodeListConfigurationAttribute();

            instance.Length(codeListConfigurationAttribute.IdLength);
        }

        public void Apply(IPropertyInstance instance)
        {
            if (!typeof(ICodeList).IsAssignableFrom(instance.EntityType) || instance.Property.Name != "Name")
            {
                return;
            }

            var codeListConfigurationAttribute = instance.EntityType.GetCustomAttribute<CodeListConfigurationAttribute>();

            if (codeListConfigurationAttribute?.NameLength.HasValue == true)
            {
                instance.Length(codeListConfigurationAttribute.NameLength.Value);
            }
        }
    }
}
