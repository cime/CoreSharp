using System.Reflection;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class CodeListAttributeConvention : IIdConvention, IClassConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            if (!typeof(ICodeList).IsAssignableFrom(instance.EntityType))
            {
                return;
            }
            var attribute = instance.EntityType.GetCustomAttribute<CodeListAttribute>(false);
            if (attribute == null)
            {
                return;
            }
            instance.Length(attribute.CodeLength);
        }

        public void Apply(IClassInstance instance)
        {
            var tableAttr = instance.EntityType.GetCustomAttribute<TableAttribute>();
            var codeListAttr = instance.EntityType.GetCustomAttribute<CodeListAttribute>();
            var isAssignableFromICodeList = instance.EntityType.IsAssignableFrom(typeof(ICodeList));

            if (tableAttr == null && (codeListAttr != null || isAssignableFromICodeList))
            {
                instance.Table($"CodeList{instance.EntityType.Name}");
            }
        }
    }
}
