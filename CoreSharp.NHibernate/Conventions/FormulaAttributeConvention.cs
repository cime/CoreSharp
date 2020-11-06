using System.Text.RegularExpressions;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Cfg;

namespace CoreSharp.NHibernate.Conventions
{
    public class FormulaAttributeConvention : AttributePropertyConvention<FormulaAttribute>
    {
        private readonly INamingStrategy _namingStrategy;
        private static readonly Regex ReplaceRegex= new Regex("(`[^`]*`)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public FormulaAttributeConvention(global::NHibernate.Cfg.Configuration configuration)
        {
            _namingStrategy = configuration.NamingStrategy;
        }
        
        protected override void Apply(FormulaAttribute attribute, IPropertyInstance instance)
        {
            var sql = ReplaceRegex.Replace(attribute.SqlFormula, m => _namingStrategy.ColumnName(m.Value.Trim('`')));
            
            instance.Formula(sql);
        }

    }
}
