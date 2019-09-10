using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class VirtualVisitor : IVisitor
    {
        public Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata projectMetadata)
        {
            var documentMetadata = projectMetadata.Documents.Single(x => x.FilePath == document.FilePath);
            var rewriter = new VirtualRewriter(documentMetadata);

            return Task.FromResult(rewriter.Visit(node));
        }
    }

    internal class VirtualRewriter : CSharpSyntaxRewriter
    {
        public List<string> Entities { get; set; } = new List<string>();
        public List<string> Overrides { get; set; } = new List<string>();

        private readonly DocumentMetadata _documentMetadata;

        public VirtualRewriter(DocumentMetadata documentMetadata)
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

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var property = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

            if (property.IsPublic() && !property.IsVirtual())
            {
                property = property.AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")));

                //Console.WriteLine("\t\t\t\t- " + property.Identifier.ToFullString());
            }

            return property;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var property = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

            if (property.IsPublic() && !property.IsVirtual())
            {
                property = property.AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")));

                //Console.WriteLine("\t\t\t\t- " + property.Identifier.ToFullString());
            }

            return property;
        }
    }
}
