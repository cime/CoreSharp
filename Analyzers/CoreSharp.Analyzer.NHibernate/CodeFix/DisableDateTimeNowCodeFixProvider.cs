using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Analyzer.NHibernate.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace CoreSharp.Analyzer.NHibernate.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisableDateTimeNowCodeFixProvider)), Shared]
    public class DisableDateTimeNowCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use DateTime.UtcNow instead of DateTime.Now";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DisableDateTimeNowAnalyzer.DiagnosticId);

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
                    createChangedDocument: c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: Title),
                diagnostic));
        }

        private static async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken);
            var repl = "DateTime.UtcNow";

            if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
            {
                repl = "System.DateTime.UtcNow";
            }

            var newtext = text.Replace(span, repl);

            return document.WithText(newtext);
        }
    }
}
