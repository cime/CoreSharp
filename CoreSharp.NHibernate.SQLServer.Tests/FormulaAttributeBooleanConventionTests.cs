using CoreSharp.NHibernate.SQLServer.Conventions;
using FluentAssertions;
using Xunit;

namespace CoreSharp.NHibernate.SQLServer.Tests
{
    public class FormulaAttributeBooleanConventionTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            var sql = "select id from test where active = true; update where keke = false; select trueTest; false";
                
            FormulaAttributeBooleanConvention.ReplaceBoolean(sql).Should().Be("select id from test where active = 1; update where keke = 0; select trueTest; 0");
            FormulaAttributeBooleanConvention.ReplaceBoolean("true").Should().Be("1");
            FormulaAttributeBooleanConvention.ReplaceBoolean("false").Should().Be("0");
            FormulaAttributeBooleanConvention.ReplaceBoolean("false or true").Should().Be("0 or 1");
            FormulaAttributeBooleanConvention.ReplaceBoolean("select * from test where active = true;").Should().Be("select * from test where active = 1;");
            
            FormulaAttributeBooleanConvention.ReplaceBoolean("True").Should().Be("True");
            FormulaAttributeBooleanConvention.ReplaceBoolean("falseTrue").Should().Be("falseTrue");
        }
    }
}
