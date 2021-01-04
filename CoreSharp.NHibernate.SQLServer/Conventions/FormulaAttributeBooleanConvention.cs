using System.Linq;
using System.Text.RegularExpressions;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.SQLServer.Conventions
{
    public class FormulaAttributeBooleanConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        private static readonly Regex Regex = new Regex("([^A-Za-z]+|^)(true|false)([^A-Za-z]+|$)", RegexOptions.Compiled);

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
                instance.Formula(ReplaceBoolean(attribute.SqlFormula));
            }
        }

        public static string ReplaceBoolean(string sql)
        {
            return Regex.Replace(sql, m =>
            {
                return m.Groups[1].Value + (m.Groups[2].Value == "true" ? "1" : "0") + m.Groups[3].Value;
            });
        }
    }
}
