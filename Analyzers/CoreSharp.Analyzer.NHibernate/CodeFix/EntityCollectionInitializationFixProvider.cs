using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Analyzer.NHibernate.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CoreSharp.Analyzer.NHibernate.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityCollectionInitializationFixProvider)), Shared]
    public class EntityCollectionInitializationFixProvider : CodeFixProvider
    {
        private const string Title = "Initialize Collection";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EntityCollectionInitializationAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            return Task.Run(() => context.RegisterCodeFix(
                CodeAction.Create(Title, c => InitializeCollectionsAsync(context.Document, diagnosticSpan, c), Title),
                diagnostic));
        }

        private static async Task<Document> InitializeCollectionsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            try
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken) as CompilationUnitSyntax;

                var nodes = root.DescendantNodes(span);
                var property = nodes.OfType<PropertyDeclarationSyntax>().SingleOrDefault();

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                var propertyInfo = semanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
                var propertType = (INamedTypeSymbol)propertyInfo.Type;
                //var propertyInterfaces = propertType.AllInterfaces;

                var collectionType = propertyInfo.Type.Name;
                var concreteCollectionType = "";

                switch (collectionType)
                {
                    case "IEnumerable":
                    case "ISet":
                    case "HashSet":
                        concreteCollectionType = "HashSet";
                        break;

                    case "IList":
                    case "List":
                        concreteCollectionType = "List";
                        break;
                }

                var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
                var genericTypeArgument = propertType.TypeArguments.First().ToDisplayString(symbolDisplayFormat);

                var newProperty = property.WithInitializer(
                        EqualsValueClause(
                            ObjectCreationExpression(
                                GenericName(
                                        Identifier(concreteCollectionType))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName(genericTypeArgument)))))
                                                    .WithArgumentList(
                                                        ArgumentList()
                                                        .WithOpenParenToken(
                                                            Token(SyntaxKind.OpenParenToken))
                                                        .WithCloseParenToken(
                                                            Token(SyntaxKind.CloseParenToken)))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));

                root = root.ReplaceNode(property, newProperty);

                return document.WithSyntaxRoot(root);
            }
            catch (System.Exception)
            {
                return document;
            }
        }
    }
}
