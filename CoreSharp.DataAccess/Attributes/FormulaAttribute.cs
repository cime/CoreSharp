using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FormulaAttribute : Attribute
    {
        public FormulaAttribute(string formula)
        {
            SqlFormula = formula;
            if (!SqlFormula.StartsWith("(") && !SqlFormula.EndsWith(")"))
                SqlFormula = string.Format("({0})", SqlFormula);
        }

        public FormulaAttribute(string formula, DatabaseType databaseType)
        {
            SqlFormula = formula;
            DatabaseType = databaseType;

            if (!SqlFormula.StartsWith("(") && !SqlFormula.EndsWith(")"))
            {
                SqlFormula = $"({SqlFormula})";
            }
        }

        public string SqlFormula { get; private set; }
        public DatabaseType? DatabaseType { get; private set; } = null;
    }
}
