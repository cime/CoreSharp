using System.Collections.Immutable;
using CoreSharp.Analyzer.NHibernate.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityPropertyNameAnalyzer : BaseAnalyzer
    {
        public const string DiagnosticId = "CS0007";
        private const string Category = "Name";
        private static readonly LocalizableString Description = "Invalid property name - should be PascalCase";
        private static readonly LocalizableString MessageFormat = "Invalid property name";
        private static readonly LocalizableString Title = "Invalid property name";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction((nodeContext) =>
            {
                if (!IsValidType(nodeContext))
                {
                    return;
                }

                var node = nodeContext.Node;

                if (!char.IsUpper(node.GetIdentifierValue()[0]) || node.GetIdentifierValue().Contains("_"))
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
                }
            }, SyntaxKind.PropertyDeclaration, SyntaxKind.MethodDeclaration);
        }
    }
}
