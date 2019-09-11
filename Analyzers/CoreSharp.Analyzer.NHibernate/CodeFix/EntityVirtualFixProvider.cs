using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Analyzer.NHibernate.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace CoreSharp.Analyzer.NHibernate.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityVirtualFixProvider)), Shared]
    public class EntityVirtualFixProvider : CodeFixProvider
    {
        private const string Title = "Add missing virtual modifier";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EntityVirtualAnalyzer.DiagnosticId);

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
                    createChangedDocument: c => AddVirtualModifierAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: Title),
                diagnostic));
        }

        private static async Task<Document> AddVirtualModifierAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken);

            var subText = text.GetSubText(span).ToString().Replace("public ", "public virtual ");

            var newtext = text.Replace(span, subText);

            return document.WithText(newtext);
        }
    }
}
