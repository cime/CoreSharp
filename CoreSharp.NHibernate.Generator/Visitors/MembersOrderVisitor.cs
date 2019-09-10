using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class MembersOrderVisitor : IVisitor
    {
        public async Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata documentMetadata)
        {
            var rewriter = new MembersOrderRewriter();

            return rewriter.Visit(node);
        }
    }

    public class MembersOrderRewriter : CSharpSyntaxRewriter
    {
        private static readonly string[] CollectionTypes = new[] { "IEnumerable", "IList", "ISet", "HashSet", "List", "ICollection" };

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            var collectionProperties = classDeclarationSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .Where(x => x.ChildNodes()
                    .OfType<GenericNameSyntax>()
                    .Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && CollectionTypes.Contains(z.ValueText))))
                .ToList();

            if (collectionProperties.Any())
            {
                var canMove = (classDeclarationSyntax.DescendantNodes()
                                   .OfType<PropertyDeclarationSyntax>()
                                   .LastOrDefault(x => !x.ChildNodes().OfType<GenericNameSyntax>().Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && z.ValueText == "ISet"))) as SyntaxNode
                               ?? classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault() as SyntaxNode) != null;

                if (canMove)
                {
                    classDeclarationSyntax = classDeclarationSyntax.RemoveNodes(collectionProperties, SyntaxRemoveOptions.KeepNoTrivia);
                }

                var lastProp = classDeclarationSyntax.DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .LastOrDefault(x => !x.ChildNodes().OfType<GenericNameSyntax>().Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && z.ValueText == "ISet"))) as SyntaxNode;
                if (lastProp != null)
                {
                    classDeclarationSyntax = classDeclarationSyntax.InsertNodesAfter(lastProp, collectionProperties);
                }
                else
                {
                    lastProp = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault() as SyntaxNode;
                    if (lastProp != null)
                    {
                        classDeclarationSyntax = classDeclarationSyntax.InsertNodesBefore(lastProp, collectionProperties);
                    }
                }
            }

            return classDeclarationSyntax;
        }
    }
}
