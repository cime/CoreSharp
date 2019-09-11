using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.Analyzer.NHibernate.Extensions
{
    public static class UsingDirectiveSyntaxExtensions
    {
        public static string GetNamespace(this UsingDirectiveSyntax usingDirectiveSyntax)
        {
            return usingDirectiveSyntax.Name.ToString();
        }
    }
}
