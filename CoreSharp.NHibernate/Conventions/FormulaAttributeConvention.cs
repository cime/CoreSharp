using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    class FormulaAttributeConvention : AttributePropertyConvention<FormulaAttribute>
    {
        protected override void Apply(FormulaAttribute attribute, IPropertyInstance instance)
        {
            instance.Formula(attribute.SqlFormula);
        }

    }
}
