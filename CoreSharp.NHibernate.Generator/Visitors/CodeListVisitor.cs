using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class CodeListVisitor : IVisitor
    {
        public async Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata projectMetadata)
        {
            var documentMetadata = projectMetadata.Documents.Single(x => x.FilePath == document.FilePath);
            var rewriter = new CodeListRewritter(documentMetadata);

            return rewriter.Visit(node);
        }
    }

    internal class CodeListRewritter : CSharpSyntaxRewriter
    {
        private readonly DocumentMetadata _documentMetadata;

        public CodeListRewritter(DocumentMetadata documentMetadata)
        {
            _documentMetadata = documentMetadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!_documentMetadata.Classes.Any(x => x.Name == node.GetIdentifierValue()))
            {
                return node;
            }

            var classMetadata = _documentMetadata.Classes.Single(x => x.Name == node.GetIdentifierValue());

            //TODO: get interface from configuration
            if (!classMetadata.Symbol.AllInterfaces.Any(x => x.Name == "ICodeList"))
            {
                return node;
            }

            var constructors = node.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();

            // protected empty constructor (for NHibernate)
            if (!constructors.Any(x => !x.DescendantNodes().OfType<ParameterSyntax>().Any() && (x.IsPublic() || x.IsProtected())))
            {
                node = node.AddMembers(ConstructorDeclaration(
                        Identifier(node.GetIdentifierValue()))
                    .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                    .WithBody(Block()));
            }

            // public constructor public X(string id) { Id = id; }
            if (!constructors.Any(x => x.DescendantNodes().OfType<ParameterSyntax>().Count() == 1 && x.IsPublic()))
            {
                node = node.AddMembers(ConstructorDeclaration(
                        Identifier(node.GetIdentifierValue()))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList<ParameterSyntax>(
                                Parameter(Identifier("id")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("Id"),
                                        IdentifierName("id")))))));
            }

            // string Name property
            if (node.ChildNodes().OfType<PropertyDeclarationSyntax>().All(x => x.GetIdentifierValue() != "Name"))
            {
                node = node.WithMembers(List<MemberDeclarationSyntax>(new[] {
                    PropertyDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.StringKeyword)),
                            Identifier("Name"))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                        .WithAccessorList(
                            AccessorList(
                                List<AccessorDeclarationSyntax>(
                                    new AccessorDeclarationSyntax[]
                                    {
                                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                                    })))}.Union(node.Members)));
                }

            return base.VisitClassDeclaration(node);
        }
    }
}
