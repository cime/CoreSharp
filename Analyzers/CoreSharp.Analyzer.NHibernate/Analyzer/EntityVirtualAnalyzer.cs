using System.Collections.Immutable;
using CoreSharp.Analyzer.NHibernate.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityVirtualAnalyzer : BaseAnalyzer
    {
        public const string DiagnosticId = "CS0001";
        private const string Category = "Modifiers";
        private static readonly LocalizableString Description = "Add missing virtual modifier";
        private static readonly LocalizableString MessageFormat = "Add missing virtual modifier";
        private static readonly LocalizableString Title = "Missing virtual modifier";
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

                if (node.IsPublic() && !node.IsStatic() && !node.IsAbstract() && !node.IsVirtual())
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
                }
            }, SyntaxKind.PropertyDeclaration, SyntaxKind.MethodDeclaration);
        }
    }
}
