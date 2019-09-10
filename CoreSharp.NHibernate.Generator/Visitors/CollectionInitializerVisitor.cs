using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;

namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class CollectionInitializerVisitor : IVisitor
    {
        private readonly MSBuildWorkspace _workspace;
        private readonly OptionSet _options;

        public CollectionInitializerVisitor(MSBuildWorkspace workspace, OptionSet options)
        {
            _workspace = workspace;
            _options = options;
        }

        public async Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata projectMetadata)
        {
            var documentMetadata = projectMetadata.Documents.Single(x => x.FilePath == document.FilePath);
            var rewriter = new CollectionInitializerRewriter(_workspace, _options, documentMetadata);

            return rewriter.Visit(node);
        }
    }

    internal class CollectionInitializerRewriter : CSharpSyntaxRewriter
    {
        private static readonly string[] CollectionTypes = new[] {"IEnumerable", "IList", "ISet", "HashSet", "List", "ICollection" };

        private readonly MSBuildWorkspace _workspace;
        private readonly OptionSet _options;
        private readonly DocumentMetadata _documentMetadata;

        public CollectionInitializerRewriter(MSBuildWorkspace workspace, OptionSet options, DocumentMetadata documentMetadata)
        {
            _workspace = workspace;
            _options = options;
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
            var classSymbol = _documentMetadata.Classes.Single(x => x.Name == property.Parent.GetIdentifierValue()).Symbol;
            var propertySymbol =  classSymbol.GetMembers().OfType<IPropertySymbol>().Single(x => x.Name == property.GetIdentifierValue());
            var propertyInterfaces = propertySymbol.Type.AllInterfaces;

            if ((CollectionTypes.Contains(propertySymbol.Type.Name)
                 || propertyInterfaces.Any(x => CollectionTypes.Contains(x.Name)))
                && propertySymbol.Type.Kind != SymbolKind.ArrayType && propertySymbol.Type.Name != "String")
            {
                var propertyTypeSymbol = (INamedTypeSymbol)propertySymbol.Type;

                if (property.ChildNodes().OfType<EqualsValueClauseSyntax>().Any() && !propertySymbol.Type.Name.Contains("IEnumerable"))
                {
                    property = property.WithType(typeof(IEnumerable<>).AsTypeSyntax(propertyTypeSymbol.TypeArguments[0].Name));

                    return property;
                }

                var propertType = (INamedTypeSymbol)propertySymbol.Type;
                var collectionType = propertType.Name;
                var concreteCollectionType = "";

                switch (collectionType)
                {
                    case "IList":
                    case "List":
                        concreteCollectionType = "List";
                        break;
                    case "IEnumerable":
                    case "ISet":
                    case "HashSet":
                    default:
                        concreteCollectionType = "HashSet";
                        break;
                }

                var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

                TypeArgumentListSyntax typeArguments = null;

                try
                {
                    typeArguments = property.ChildNodes().OfType<GenericNameSyntax>().First().TypeArgumentList;
                }
                catch (Exception)
                {
                    var genericTypeArgument = propertType.TypeArguments.First().ToDisplayString(symbolDisplayFormat);
                    typeArguments = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(genericTypeArgument)));
                }

                var newProperty = property
                    .WithType(typeof(IEnumerable<>).AsTypeSyntax(propertyTypeSymbol.TypeArguments[0].Name))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List(new []
                            {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            })))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier(concreteCollectionType))
                                        .WithTypeArgumentList(typeArguments))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList()
                                        .WithOpenParenToken(
                                            SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                        .WithCloseParenToken(
                                            SyntaxFactory.Token(SyntaxKind.CloseParenToken)))))
                    .WithSemicolonToken(
                        SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.Space);

                var newNode = Formatter.Format(newProperty, _workspace, _options).WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space, SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                return newNode;
            }

            return property;
        }
    }
}
