using System;
using System.Linq;
using System.Text.RegularExpressions;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Cfg;

namespace CoreSharp.NHibernate.Conventions
{
    public class FormulaAttributeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        private readonly IDatabaseTypeAccessor _databaseTypeAccessor;
        private readonly INamingStrategy _namingStrategy;
        private static readonly Regex ReplaceRegex= new Regex("(`[^`]*`)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public FormulaAttributeConvention(global::NHibernate.Cfg.Configuration configuration, IDatabaseTypeAccessor databaseTypeAccessor)
        {
            _databaseTypeAccessor = databaseTypeAccessor;
            _namingStrategy = configuration.NamingStrategy;
        }

        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(property => property.Property.MemberInfo.GetCustomAttributes(true).Any(x => x is FormulaAttribute));
        }

        public void Apply(IPropertyInstance instance)
        {
            var attributes = instance.Property.MemberInfo.GetCustomAttributes(true)
                .Where(x => x is FormulaAttribute)
                .Select(x => x as FormulaAttribute)
                .ToList();

            foreach (var attribute in attributes)
            {
                if (attribute.DatabaseType == null || attribute.DatabaseType == _databaseTypeAccessor.GetDatabaseType())
                {
                    var sql = ReplaceRegex.Replace(attribute.SqlFormula, m => _namingStrategy.ColumnName(m.Value.Trim('`')));
                    instance.Formula(sql);
                }
            }
        }
    }
}
