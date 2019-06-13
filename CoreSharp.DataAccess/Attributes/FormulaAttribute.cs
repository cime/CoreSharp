using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormulaAttribute : Attribute
    {
        public FormulaAttribute(string formula)
        {
            SqlFormula = formula;
            if (!SqlFormula.StartsWith("(") && !SqlFormula.EndsWith(")"))
                SqlFormula = string.Format("({0})", SqlFormula);
        }

        public string SqlFormula { get; private set; }
    }
}
