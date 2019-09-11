using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Analyzer.NHibernate.Analyzer;
using CoreSharp.Analyzer.NHibernate.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CoreSharp.Analyzer.NHibernate.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityCollectionMethodsFixProvider)), Shared]
    public class EntityCollectionMethodsFixProvider : CodeFixProvider
    {
        private const string Title = "Generate Collection Methods";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EntityCollectionMethodsAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            return Task.Run(() => context.RegisterCodeFix(
                CodeAction.Create(Title, c => GenerateCollectionMethodsAsync(context.Document, diagnosticSpan, c), Title),
                diagnostic));
        }

        private static async Task<Document> GenerateCollectionMethodsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            try
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken) as CompilationUnitSyntax;

                var nodes = root.DescendantNodes(span);
                var property = nodes.OfType<PropertyDeclarationSyntax>().Single();
                var @class = (ClassDeclarationSyntax)property.Parent;
                var newClass = @class;

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                var classSymbol = semanticModel.GetDeclaredSymbol(@class);
                var propertySymbol = semanticModel.GetDeclaredSymbol(property);
                var propertyType = (INamedTypeSymbol)propertySymbol.Type;
                var childType = propertyType.TypeArguments.Single();
                var childTypeName = childType.Name;

                var propertyName = propertySymbol.Name;
                var propertyNameSingular = PluralizationServiceInstance.Instance.Singularize(propertySymbol.Name);

                var parentName = @class.GetIdentifierValue();

                var addMethodName = "Add" + propertyNameSingular;
                var removeMethodName = "Remove" + propertyNameSingular;
                var clearMethodName = "Clear" + propertySymbol.Name;

                if (!classSymbol.MemberNames.Contains(addMethodName))
                {
                    newClass = HandleAddMethod(newClass, parentName, propertyName, childTypeName, addMethodName, removeMethodName);
                }

                if (!classSymbol.MemberNames.Contains(removeMethodName))
                {
                    newClass = HandleRemoveMethod(newClass, parentName, propertyName, childTypeName, removeMethodName);
                }

                if (!classSymbol.MemberNames.Contains(clearMethodName))
                {
                    newClass = HandleClearMethod(newClass, parentName, propertyName, clearMethodName, removeMethodName);
                }

                root = root.ReplaceNode(@class, newClass);

                return document.WithSyntaxRoot(root);
            }
            catch (System.Exception)
            {
                return document;
            }
        }

        private static ClassDeclarationSyntax HandleAddMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string childTypeName, string addMethodName, string removeMethodName)
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

        private static ClassDeclarationSyntax HandleRemoveMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string childTypeName, string addMethodName)
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

        private static ClassDeclarationSyntax HandleClearMethod(ClassDeclarationSyntax node, string parentName, string propertyName, string clearMethodName, string removeMethodName)
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
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(propertyName),
                                        IdentifierName("ToList"))),
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
