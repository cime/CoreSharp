using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Extensions;
using CoreSharp.NHibernate.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CoreSharp.NHibernate.Generator.Visitors
{
    internal class CollectionMethodsVisitor : IVisitor
    {
        public async Task<SyntaxNode> Visit(SyntaxNode node, Document document, ProjectMetadata projectMetadata)
        {
            var documentMetadata = projectMetadata.Documents.Single(x => x.FilePath == document.FilePath);
            var rewriter = new CollectionMethodsRewriter(documentMetadata, projectMetadata);

            return rewriter.Visit(node);
        }
    }

    internal class CollectionMethodsRewriter : CSharpSyntaxRewriter
    {
        private static readonly string[] CollectionTypes = new[] {"IEnumerable", "IList", "ISet", "HashSet", "List", "ICollection" };

        private readonly DocumentMetadata _documentMetadata;
        private readonly ProjectMetadata _projectMetadata;
        private readonly PluralizationServiceInstance _pluralizationService = new PluralizationServiceInstance(new CultureInfo("en-US"));

        private bool _hasChanges = false;

        public CollectionMethodsRewriter(DocumentMetadata documentMetadata, ProjectMetadata projectMetadata)
        {
            _documentMetadata = documentMetadata;
            _projectMetadata = projectMetadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!_documentMetadata.Classes.Any(x => x.Name == node.GetIdentifierValue()))
            {
                return node;
            }

            var className = node.GetIdentifierValue();

            var cls = _documentMetadata.Classes.Single(x => x.Name == node.GetIdentifierValue());

            // Skip method generation for views
            if (cls.TableAttribute != null && cls.TableAttribute.View)
            {
                return node;
            }

            var classSymbol = cls.Symbol;
            var overrideMetadata = _documentMetadata.Overrides.Where(x => x.EntityType == classSymbol.ToDisplayString()).ToList();
            var expressions = overrideMetadata.SelectMany(x => x.Expressions).ToList();

            var properties = node.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(p =>
            {
                var propertySymbol = classSymbol.GetMembers().OfType<IPropertySymbol>().Single(x => x.Name == p.GetIdentifierValue());
                var propertyInterfaces = propertySymbol.Type.AllInterfaces;

                if ((CollectionTypes.Contains(propertySymbol.Type.Name)
                     || propertyInterfaces.Any(x => CollectionTypes.Contains(x.Name)))
                    && propertySymbol.Type.Kind != SymbolKind.ArrayType && propertySymbol.Type.Name != "String")
                {
                    return propertySymbol;
                }

                return null;
            }).Where(x => x != null).ToList();

            _hasChanges = properties.Any();

            foreach (var propertySymbol in properties)
            {
                var propertyName = propertySymbol.Name;
                var propertyType = (INamedTypeSymbol)propertySymbol.Type;
                var childTypeName = propertyType.TypeArguments.First().Name;
                var addMethodName = "Add" + _pluralizationService.Singularize(propertyName);
                var removeMethodName = "Remove" + _pluralizationService.Singularize(propertyName);
                var clearMethodName = "Clear" + propertyName;

                //TODO: remove existing methods
                var debug = true;

                if (debug)
                {
                    foreach (var methodName in new[] { addMethodName, removeMethodName, clearMethodName })
                    {
                        if (classSymbol.HasMethod(methodName))
                        {
                            node = node.RemoveMethod(methodName);
                        }
                    }
                }

                var isBidirectional = true;

                if (isBidirectional)
                {
                    var parentName = node.GetIdentifierValue();

                    if (cls.TableAttribute != null && !string.IsNullOrEmpty(cls.TableAttribute.Name))
                    {
                        parentName = cls.TableAttribute.Name;
                    }

                    if (expressions.Any(x => x.PropertyName == propertyName))
                    {
                        parentName = expressions.Last(x => x.PropertyName == propertyName).ReferenceName;
                    }

                    if (debug || !classSymbol.MemberNames.Contains(addMethodName))
                    {
                        node = HandleAddMethod(node, parentName, propertyName, childTypeName, addMethodName, removeMethodName, expressions);
                    }

                    if (debug || !classSymbol.MemberNames.Contains(removeMethodName))
                    {
                        node = HandleRemoveMethod(node, parentName, propertyName, childTypeName, removeMethodName, expressions);
                    }

                    if (debug || !classSymbol.MemberNames.Contains(clearMethodName))
                    {
                        node = HandleClearMethod(node, parentName, propertyName, clearMethodName, removeMethodName, expressions);
                    }
                }
            }

            return base.VisitClassDeclaration(node);
        }

        private static ClassDeclarationSyntax HandleAddMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string childTypeName, string addMethodName, string removeMethodName, List<OverrideExpression> expressions)
        {
            node = node.AddMembers(MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier(addMethodName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(Identifier("item"))
                                .WithType(
                                    IdentifierName(childTypeName)))))
                .WithBody(Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName("AddOneToMany")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    SimpleLambdaExpression(
                                                        Parameter(
                                                            Identifier("o")),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("o"),
                                                            IdentifierName(propertyName)))),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("item")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    SimpleLambdaExpression(
                                                        Parameter(
                                                            Identifier("o")),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("o"),
                                                            IdentifierName(parentName)))),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    SimpleLambdaExpression(
                                                        Parameter(
                                                            Identifier("o")),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("o"),
                                                            IdentifierName(removeMethodName))))}))))))));

            return node;
        }

        private static ClassDeclarationSyntax HandleRemoveMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string childTypeName, string addMethodName, List<OverrideExpression> expressions)
        {
            node = node.AddMembers(MethodDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.VoidKeyword)),
                            Identifier(addMethodName))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                        .WithParameterList(
                            ParameterList(
                                SingletonSeparatedList<ParameterSyntax>(
                                    Parameter(Identifier("item"))
                                        .WithType(
                                            IdentifierName(childTypeName)))))
                        .WithBody(Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName("RemoveOneToMany")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    SimpleLambdaExpression(
                                                        Parameter(
                                                            Identifier("o")),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("o"),
                                                            IdentifierName(propertyName)))),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("item")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    SimpleLambdaExpression(
                                                        Parameter(
                                                            Identifier("o")),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("o"),
                                                            IdentifierName(parentName))))}))))))));

            return node;
        }

        private static ClassDeclarationSyntax HandleClearMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string clearMethodName, string removeMethodName, List<OverrideExpression> expressions)
        {
            node = node.AddMembers(MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier(clearMethodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ForEachStatement(
                                IdentifierName("var"),
                                Identifier("item"),
                                IdentifierName(propertyName),
                                Block(
                                    SingletonList<StatementSyntax>(
                                        ExpressionStatement(
                                            InvocationExpression(
                                                    IdentifierName(removeMethodName))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                            Argument(
                                                                IdentifierName("item")))))))))))));

            return node;
        }
    }
}
