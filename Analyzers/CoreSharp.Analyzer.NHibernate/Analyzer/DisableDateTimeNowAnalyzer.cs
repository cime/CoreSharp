using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DisableDateTimeNowAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CS0002";
        private const string Category = "Illegal Method Calls";
        private static readonly LocalizableString Description = "Use DateTime.UtcNow instead of DateTime.Now";
        private static readonly LocalizableString MessageFormat = "Use DateTime.UtcNow instead of DateTime.Now";
        private static readonly LocalizableString Title = "Illegal use of DateTime.Now";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction((compilationStartContext) =>
            {
                var dateTimeType = compilationStartContext.Compilation.GetTypeByMetadataName("System.DateTime");

                compilationStartContext.RegisterSyntaxNodeAction((analysisContext) =>
                        {
                            var invocations = analysisContext.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
                            foreach (var invocation in invocations)
                            {
                                ExpressionSyntax e;
                                if (invocation.Expression is MemberAccessExpressionSyntax)
                                {
                                    e = (MemberAccessExpressionSyntax)invocation.Expression;
                                }
                                else if (invocation.Expression is IdentifierNameSyntax)
                                {
                                    e = (IdentifierNameSyntax)invocation.Expression;
                                }
                                else
                                    continue;

                                if (e == null)
                                    continue;
                                var typeInfo = analysisContext.SemanticModel.GetTypeInfo(e).Type as INamedTypeSymbol;
                                if (typeInfo?.ConstructedFrom == null)
                                    continue;

                                if (!typeInfo.ConstructedFrom.Equals(dateTimeType))
                                    continue;
                                if (invocation.Name.ToString() == "Now")
                                {
                                    analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
                                }
                            }
                        }, SyntaxKind.MethodDeclaration);
            });
        }
    }
}
