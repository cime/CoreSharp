using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSharp.Analyzer.NHibernate.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoreSharp.Analyzer.NHibernate.Analyzer
{
    public abstract class BaseAnalyzer : DiagnosticAnalyzer
    {
        protected static readonly SymbolDisplayFormat SymbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        protected const string ConfigFileName = "analyzers.config";
        protected IList<string> ValidTypes = new List<string>() { "CoreSharp.DataAccess.IEntity", "CoreSharp.DataAccess.ICodeList" };

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction((compilationStartContext) =>
            {
                GetConfiguration(compilationStartContext);
            });
        }

#pragma warning disable RS1012 // Start action has no registered actions.
        protected AnalyzersConfig GetConfiguration(CompilationStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            // TODO: check how to do a smart cache
            var file = context.Options.AdditionalFiles.SingleOrDefault(f => string.Compare(Path.GetFileName(f.Path), ConfigFileName, StringComparison.OrdinalIgnoreCase) == 0);
            if (file != null)
            {
                var config = AnalyzersConfig.Deserialize(file.GetText().ToString());

                ValidTypes = config.VirtualModifierAnalyzerValidTypes;

                return config;
            }

            return new AnalyzersConfig();
        }

        protected bool IsValidType(INamedTypeSymbol cls)
        {
            var interfaces = cls.AllInterfaces.Select(x => x.ToDisplayString(SymbolDisplayFormat));

            if (interfaces.Any(x => ValidTypes.Contains(x)))
            {
                return true;
            }

            return false;
        }

        protected bool IsValidType(SyntaxNodeAnalysisContext nodeContext)
        {
            var classNode = nodeContext.Node.Parent as ClassDeclarationSyntax;

            if (classNode == null)
            {
                return false;
            }

            var interfaces = nodeContext.SemanticModel.GetDeclaredSymbol(classNode).AllInterfaces.Select(x => x.ToDisplayString(SymbolDisplayFormat));

            if (interfaces.Any(x => ValidTypes.Contains(x)))
            {
                return true;
            }

            return false;
        }

        protected bool IsValidType(ClassDeclarationSyntax classSyntax, SemanticModel semanticModel)
        {
            var classNode = classSyntax;

            if (classNode == null)
            {
                return false;
            }

            var interfaces = semanticModel.GetDeclaredSymbol(classNode).AllInterfaces.Select(x => x.ToDisplayString(SymbolDisplayFormat));

            if (interfaces.Any(x => ValidTypes.Contains(x)))
            {
                return true;
            }

            return false;
        }
    }
}
