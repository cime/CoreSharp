using System.Reflection;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class CodeListConfigurationAttributeConvention : IIdConvention, IPropertyConvention, IClassConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            if (!typeof(ICodeList).IsAssignableFrom(instance.EntityType))
            {
                return;
            }
            var attribute = instance.EntityType.GetCustomAttribute<CodeListConfigurationAttribute>(false);
            if (attribute == null)
            {
                return;
            }
            instance.Length(attribute.CodeLength);
        }

        public void Apply(IClassInstance instance)
        {
            var tableAttr = instance.EntityType.GetCustomAttribute<TableAttribute>();
            var codeListAttr = instance.EntityType.GetCustomAttribute<CodeListConfigurationAttribute>(false);
            var isAssignableFromICodeList = instance.EntityType.IsAssignableFrom(typeof(ICodeList));

            if (isAssignableFromICodeList)
            {
                if (tableAttr == null && codeListAttr?.CodeListPrefix == true)
                {
                    instance.Table($"CodeList{instance.EntityType.Name}");
                }
            }
        }

        public void Apply(IPropertyInstance instance)
        {
            if (!typeof(ICodeList).IsAssignableFrom(instance.EntityType) || instance.Name != "Name")
            {
                return;
            }

            var codeListAttr = instance.EntityType.GetCustomAttribute<CodeListConfigurationAttribute>(false);

            if (codeListAttr?.NameLength != null)
            {
                instance.Length(codeListAttr.NameLength.Value);
            }
        }
    }
}
