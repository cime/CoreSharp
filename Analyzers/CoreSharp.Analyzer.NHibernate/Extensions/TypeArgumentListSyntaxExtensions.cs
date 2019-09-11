using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.Analyzer.NHibernate.Extensions
{
    public static class TypeArgumentListSyntaxExtensions
    {
        public static string GetFirstGenericArgument(this TypeArgumentListSyntax typeArgumentListSyntaxExtensions)
        {
            var childNodes = typeArgumentListSyntaxExtensions.ChildNodes();

            foreach (var syntaxNode in childNodes)
            {
                if (syntaxNode is QualifiedNameSyntax)
                {
                    return string.Join(".", syntaxNode.ChildNodes().OfType<IdentifierNameSyntax>().Select(x => x.Identifier.ToString()));
                }
                else if(syntaxNode is IdentifierNameSyntax)
                {
                    return (syntaxNode as IdentifierNameSyntax).Identifier.ToString();
                }
            }
            
            return null;
        }
    }
}
