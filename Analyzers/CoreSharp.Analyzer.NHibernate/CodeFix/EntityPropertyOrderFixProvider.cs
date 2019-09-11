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

namespace CoreSharp.Analyzer.NHibernate.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityPropertyOrderFixProvider)), Shared]
    public class EntityPropertyOrderFixProvider : CodeFixProvider
    {
        private const string Title = "Reorder properties";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EntityPropertyOrderAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            return Task.Run(() => context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReorderPropertiesAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: Title),
                diagnostic));
        }

        private static async Task<Document> ReorderPropertiesAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            try
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var @class in classes)
                {
                    var classDeclarationSyntax = @class;

                    var isetProperties = classDeclarationSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                                                    .Where(x => x.ChildNodes()
                                                                 .OfType<GenericNameSyntax>()
                                                                 .Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && (z.ValueText == "ISet" || z.ValueText == "HashSet" || z.ValueText == "IList" || z.ValueText == "List"))))
                                                    .ToList();

                    if (isetProperties.Any())
                    {
                        //var first = isetProperties.First();
                        //var newFirst = first.WithLeadingTrivia(TriviaList(

                        //    RegionDirectiveTrivia(true)
                        //        .WithHashToken(
                        //            Token(SyntaxKind.HashToken))
                        //        .WithRegionKeyword(
                        //            Token(
                        //                TriviaList(),
                        //                SyntaxKind.RegionKeyword,
                        //                TriviaList(
                        //                    Space)))
                        //        .WithEndOfDirectiveToken(
                        //            Token(
                        //                TriviaList(
                        //                    PreprocessingMessage("Test fooo")),
                        //                SyntaxKind.EndOfDirectiveToken,
                        //                TriviaList(
                        //                    LineFeed))))

                        //));

                        var canMove = (classDeclarationSyntax.DescendantNodes()
                                        .OfType<PropertyDeclarationSyntax>()
                                        .LastOrDefault(x => !x.ChildNodes().OfType<GenericNameSyntax>().Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && z.ValueText == "ISet"))) as SyntaxNode
                                        ?? classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault() as SyntaxNode) != null;

                        if (canMove)
                        {
                            classDeclarationSyntax = classDeclarationSyntax.RemoveNodes(isetProperties, SyntaxRemoveOptions.KeepNoTrivia);
                        }

                        var lastProp = classDeclarationSyntax.DescendantNodes()
                                        .OfType<PropertyDeclarationSyntax>()
                                        .LastOrDefault(x => !x.ChildNodes().OfType<GenericNameSyntax>().Any(y => y.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && z.ValueText == "ISet"))) as SyntaxNode;
                        if (lastProp != null)
                        {
                            classDeclarationSyntax = classDeclarationSyntax.InsertNodesAfter(lastProp, isetProperties);
                        }
                        else
                        {
                            lastProp = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault() as SyntaxNode;
                            if (lastProp != null)
                            {
                                classDeclarationSyntax = classDeclarationSyntax.InsertNodesBefore(lastProp, isetProperties);
                            }
                        }
                    }

                    root = root.ReplaceNode(@class, classDeclarationSyntax);
                }

                return document.WithSyntaxRoot(root);
            }
            catch (System.Exception)
            {
                return document;
            }
        }
    }
}
