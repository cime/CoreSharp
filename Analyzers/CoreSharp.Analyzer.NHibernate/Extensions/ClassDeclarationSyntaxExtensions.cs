using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.Analyzer.NHibernate.Extensions
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static bool HasGeneratedAttribute(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.ChildNodes().OfType<AttributeListSyntax>()
                .Any(x => x.DescendantNodes().OfType<AttributeSyntax>()
                    .Any(a => a.DescendantNodes().OfType<QualifiedNameSyntax>()
                        .Any(qn => qn.DescendantTokens()
                            .Any(t => t.Kind() == SyntaxKind.IdentifierToken && (t.ValueText == "Generated" || t.ValueText == "DebuggerNonUserCode")))));
        }
    }
}
