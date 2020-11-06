using System.Text.RegularExpressions;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.SQLServer.Conventions
{
    public class FormulaAttributeBooleanConvention : AttributePropertyConvention<FormulaAttribute>
    {
        private static Regex Regex = new Regex("([^A-Za-z]+|^)(true|false)([^A-Za-z]+|$)", RegexOptions.Compiled);
        
        protected override void Apply(FormulaAttribute attribute, IPropertyInstance instance)
        {
            instance.Formula(ReplaceBoolean(attribute.SqlFormula));
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
