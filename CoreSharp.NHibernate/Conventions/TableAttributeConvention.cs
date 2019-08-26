using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class TableAttributeConvention : IClassConvention
    {
        public void Apply(IClassInstance instance)
        {
            var tableAttr = instance.EntityType.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {

                if (!string.IsNullOrEmpty(tableAttr.Name))
                {
                    instance.Table(tableAttr.Name);
                }

                if (!string.IsNullOrEmpty(tableAttr.Prefix))
                {
                    instance.Table($"{tableAttr.Prefix}{instance.TableName}");
                }

                if (!string.IsNullOrEmpty(tableAttr.Suffix))
                {
                    instance.Table($"{instance.TableName}{tableAttr.Suffix}");
                }

                if (tableAttr.View)
                {
                    instance.ReadOnly();
                    instance.SchemaAction.None();
                }
            }
        }
    }
}
