using System.Collections.Immutable;
using System.Linq;
using CoreSharp.Analyzer.NHibernate.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityPropertyOrderAnalyzer : BaseAnalyzer
    {
        public const string DiagnosticId = "CS0003";
        private const string Category = "Members Order";
        private static readonly LocalizableString Description = "Reorder properties";
        private static readonly LocalizableString MessageFormat = "Reorder properties";
        private static readonly LocalizableString Title = "Reorder properties";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction((compilationStartContext) =>
            {
                GetConfiguration(compilationStartContext);

                compilationStartContext.RegisterSyntaxTreeAction((treeContext) =>
                {
                    if (treeContext.Tree.FilePath.IsGeneratedFile())
                    {
                        return;
                    }

                    var root = treeContext.Tree.GetRoot();

                    var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                    foreach (var @class in classes)
                    {
                        if (!IsValidType(@class, compilationStartContext.Compilation.GetSemanticModel(@class.SyntaxTree)))
                        {
                            continue;
                        }

                        if (@class.HasGeneratedAttribute())
                        {
                            continue;
                        }

                        var properties = @class.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                        var propertyTypes = properties.Select(x =>
                        {
                            var genericNames = x.ChildNodes().OfType<GenericNameSyntax>().ToList();

                            if (!genericNames.Any())
                            {
                                return false;
                            }

                            return genericNames.Any(g => g.ChildTokens().Any(z => z.Kind() == SyntaxKind.IdentifierToken && (z.ValueText == "ISet" || z.ValueText == "HashSet" || z.ValueText == "IList" || z.ValueText == "List")));
                        });

                        var genericFound = false;
                        foreach (var prop in propertyTypes)
                        {
                            if (genericFound && !prop)
                            {
                                treeContext.ReportDiagnostic(Diagnostic.Create(Rule, @class.GetLocation()));

                                return;
                            }

                            if (prop)
                            {
                                genericFound = true;
                            }
                        }
                    }
                });
            });
        }
    }
}
