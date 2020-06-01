using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using CoreSharp.Analyzer.NHibernate.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityCollectionMethodsAnalyzer : BaseAnalyzer
    {
        public const string DiagnosticId = "CS0006";
        private const string Category = "Property Initialization";
        private static readonly LocalizableString Description = "Generate Collection Methods";
        private static readonly LocalizableString MessageFormat = "Generate Collection Methods";
        private static readonly LocalizableString Title = "Generate Collection Methods";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

                    var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

                    var semanticModel = compilationStartContext.Compilation.GetSemanticModel(treeContext.Tree);

                    foreach (var classDeclarationSyntax in classes)
                    {
                        if (classDeclarationSyntax.HasGeneratedAttribute())
                        {
                            continue;
                        }

                        var cls = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                        if (!IsValidType(cls))
                        {
                            continue;
                        }


                        var members = cls.GetMembers();
                        var properties = members.OfType<IPropertySymbol>().ToList();
                        var methods = members.OfType<IMethodSymbol>().ToList();

                        foreach (var propertySymbol in properties)
                        {
                            var propertyInterfaces = propertySymbol.Type.AllInterfaces;
                            var namedTypeSymbol = propertySymbol.Type as INamedTypeSymbol;

                            if (namedTypeSymbol?.IsGenericType == true &&
                                (propertySymbol.Type.Name == "IEnumerable"
                                || propertySymbol.Type.Name == "ISet"
                                || propertySymbol.Type.Name == "IList"
                                || propertySymbol.Type.Name == "HashSet"
                                || propertySymbol.Type.Name == "List"
                                || propertyInterfaces.Any(x => x.Name == "ISet" || x.Name == "IList" || x.Name == "IEnumerable"))
                                && propertySymbol.Type.Kind != SymbolKind.ArrayType)
                            {
                                var propertyNameSingular = PluralizationServiceInstance.Instance.Singularize(propertySymbol.Name);
                                var methodNames = new [] { $"Add{propertyNameSingular}", $"Remove{propertyNameSingular}", $"Clear{propertySymbol.Name}" };
                                var notFound = methodNames.Where(x => !methods.Any(m => m.Name == x)).ToList();

                                if (notFound.Any())
                                {
                                    var propSyntax = classDeclarationSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                                        .SingleOrDefault(x => x.ChildTokens().Any(t => t.Kind() == SyntaxKind.IdentifierToken && t.Value.ToString() == propertySymbol.Name));

                                    var hasSetter = propSyntax?.DescendantNodes()
                                        .OfType<AccessorDeclarationSyntax>()
                                        .Any(x => x.Kind() == SyntaxKind.SetAccessorDeclaration) ?? false;

                                    if (hasSetter)
                                    {
                                        treeContext.ReportDiagnostic(Diagnostic.Create(Rule, propSyntax.GetLocation()));
                                    }
                                }
                            }
                        }
                    }
                });
            });
        }
    }
}
