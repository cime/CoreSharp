using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class AutoPropertyVisitorVisitor : IVisitor
    {
        public async Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata projectMetadata)
        {
            var documentMetadata = projectMetadata.Documents.Single(x => x.FilePath == document.FilePath);
            var rewriter = new AutoPropertyVisitorRewriter(documentMetadata);

            return rewriter.Visit(node);
        }
    }

    internal class AutoPropertyVisitorRewriter : CSharpSyntaxRewriter
    {
        private readonly DocumentMetadata _documentMetadata;

        public AutoPropertyVisitorRewriter(DocumentMetadata documentMetadata)
        {
            _documentMetadata = documentMetadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!_documentMetadata.Classes.Any(x => x.Name == node.GetIdentifierValue()))
            {
                return node;
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var property = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

            if (property.DescendantNodes().OfType<ArrowExpressionClauseSyntax>().Any())
            {
                return property;
            }

            var blocks = property.DescendantNodes().OfType<AccessorDeclarationSyntax>().Select(x => x.DescendantNodes().OfType<BlockSyntax>()).Where(x => x.Any()).ToList();
            var isAutoProperty = blocks.Count == 0;
            if (!isAutoProperty)
            {
                property = property.WithAccessorList(
                AccessorList(
                    List<AccessorDeclarationSyntax>(
                        new []
                        {
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        })));
            }

            return property;
        }
    }
}
