using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable once CheckNamespace
namespace Microsoft.CodeAnalysis.CSharp
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

        public static ClassDeclarationSyntax RemoveMethod(this ClassDeclarationSyntax classDeclarationSyntax, string methodName)
        {
            return classDeclarationSyntax
                    .RemoveNodes(
                        classDeclarationSyntax.ChildNodes().OfType<MethodDeclarationSyntax>().Where(x => x.GetIdentifierValue() == methodName),
                        SyntaxRemoveOptions.KeepTrailingTrivia
                    );
        }
    }
}
