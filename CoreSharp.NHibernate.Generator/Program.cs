using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSharp.NHibernate.Generator.Models;
using CoreSharp.NHibernate.Generator.Visitors;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace CoreSharp.NHibernate.Generator
{
    class Program
    {
        private static readonly Container Container = new Container();

        private static readonly string OverrideInterface = "FluentNHibernate.Automapping.Alterations.IAutoMappingOverride";
        private static readonly Regex OverrideRegex = new Regex("FluentNHibernate.Automapping.Alterations.IAutoMappingOverride<([^>,]*)>", RegexOptions.Compiled);

        public static readonly string[] CollectionTypes = { "IEnumerable", "IList", "ISet", "HashSet", "List", "ICollection" };

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Use as NHibernateGenerator.exe SolutionPath");
                return;
            }

            var solutionPath = args[0];
            if (string.IsNullOrEmpty(solutionPath) || !File.Exists(solutionPath))
            {
                Console.WriteLine($"Solution {solutionPath} does not exists!");
                return;
            }

            MSBuildLocator.RegisterDefaults();
#if NETCOREAPP
			// Hack to be able to open an project that have one or more project references
			var projectBuildManagerType = typeof(MSBuildProjectLoader).Assembly.GetType("Microsoft.CodeAnalysis.MSBuild.Build.ProjectBuildManager");
			var field = projectBuildManagerType.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
				.FirstOrDefault(o => o.Name == "s_defaultGlobalProperties");
			if (field != null)
			{
				var globalProperties = (ImmutableDictionary<string, string>)field.GetValue(null);
				field.SetValue(null, globalProperties.SetItem("BuildingInsideVisualStudio", "false"));
			}
#endif

            Task.Run(async () =>
            {
                try
                {
                    await MainAsync(solutionPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }).Wait();
        }

        private static async Task MainAsync(string solutionPath)
        {
            var sw = Stopwatch.StartNew();

            Container.Collection.Append(typeof(IVisitor), typeof(AutoPropertyVisitorVisitor));
            Container.Collection.Append(typeof(IVisitor), typeof(VirtualVisitor));
            Container.Collection.Append(typeof(IVisitor), typeof(CollectionInitializerVisitor));
            Container.Collection.Append(typeof(IVisitor), typeof(CollectionMethodsVisitor));
            Container.Collection.Append(typeof(IVisitor), typeof(CodeListVisitor));
            Container.Collection.Append(typeof(IVisitor), typeof(MembersOrderVisitor));

            Console.WriteLine($"Loading Solution {solutionPath}...");

            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += WorkspaceFailed;
            var options = workspace.Options
                .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, Environment.NewLine)
                .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
                .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
                .WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            //var originalSolution = solution;
            //var project = await workspace.OpenProjectAsync(@"C:\Projects\pcs_baku_merge\PCS.Domain\PCS.Domain.csproj");

            Console.WriteLine("Solution loaded in {0:n2}s ({1}ms)", sw.ElapsedMilliseconds / 1000.0f, sw.ElapsedMilliseconds);

            Container.RegisterSingleton(() => workspace);
            Container.RegisterSingleton(() => options);

            Container.Verify();

            foreach (var projectId in solution.ProjectIds)
            {
                var project = solution.GetProject(projectId);

                var projectDirectory = Path.GetDirectoryName(project.FilePath);
                var configFilePath = Path.Combine(projectDirectory, "NHibernateGenerator.xml");
                var hasConfigFile = File.Exists(configFilePath);

                if (hasConfigFile)
                {
                    var metadata = new ProjectMetadata()
                    {
                        Name = project.Name,
                        Settings = Settings.Deserialize(configFilePath)
                    };

                    var sw2 = Stopwatch.StartNew();
                    Console.WriteLine($"Parsing metadata for project {project.Name}...");
                    Task.WaitAll(project.Documents.Where(x => IsValidDocumentName(x.Name)).Select(document => Task.Run(async () =>
                    {
                        var root = await document.GetSyntaxRootAsync();
                        var semanticModel = await document.GetSemanticModelAsync();

                        var classMetadataList = new List<ClassMetadata>();
                        var overrideMetadataList = new List<OverrideClassMetadata>();
                        foreach (var classDeclarationSyntax in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                        {
                            var symbol = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                            if (symbol.AllInterfaces.Any(x => x.ToDisplayString().StartsWith(OverrideInterface)))
                            {
                                //TODO: multiple overrides on the same class?
                                var overrideType = symbol.AllInterfaces.Where(x => x.ToDisplayString().StartsWith(OverrideInterface)).Select(x => x.ToDisplayString()).First();
                                var entityType = OverrideRegex.Match(overrideType).Groups[1].Value;

                                overrideMetadataList.Add(new OverrideClassMetadata()
                                {
                                    EntityType = entityType,
                                    Name = symbol.Name,
                                    Symbol = symbol,
                                    Method = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().First(x => x.GetIdentifierValue() == "Override" && x.IsPublic())
                                });
                            }

                            if (symbol.AllInterfaces.Any(x => x.ToDisplayString() == metadata.Settings.BaseEntityType))
                            {
                                var classMetadata = new ClassMetadata()
                                {
                                    Name = symbol.Name,
                                    Symbol = symbol,
                                    TableAttribute = symbol.GetAttributes().Where(x => x.AttributeClass.ToDisplayString() == "CoreSharp.DataAccess.Attributes.TableAttribute").Select(x => new TableAttribute(x)).SingleOrDefault(),
                                    Properties = symbol.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select((x) => new PropertyMetadata()
                                        {
                                            Name = x.Name,
                                            Type = ((IPropertySymbol) x).Type as INamedTypeSymbol,
                                            Symbol = (IPropertySymbol) x
                                        }).ToList(),
                                    Methods = symbol.GetMembers().Where(x => x.Kind == SymbolKind.Method && ((IMethodSymbol) x).MethodKind == MethodKind.Ordinary)
                                        .Select((x) => new MethodMetadata()
                                            {
                                                Name = x.Name,
                                                Type = ((IMethodSymbol) x).ReturnType as INamedTypeSymbol,
                                                Symbol = (IMethodSymbol) x
                                            }).ToList()
                                };

                                classMetadataList.Add(classMetadata);
                            }
                        }

                        metadata.Documents.Add(new DocumentMetadata()
                        {
                            FilePath = document.FilePath,
                            Classes = classMetadataList,
                            Overrides = overrideMetadataList
                        });
                    })).ToArray());
                    Console.WriteLine($"Metadata for project {project.Name} parsed in {0:n2}s ({1}ms)", sw2.ElapsedMilliseconds / 1000.0f, sw2.ElapsedMilliseconds);

                    var sw3 = Stopwatch.StartNew();
                    Console.WriteLine("Applying mutations...");

                    foreach (var documentId in project.DocumentIds)
                    {
                        var document = project.GetDocument(documentId);

                        if (!IsValidDocumentName(document.Name))
                        {
                            continue;
                        }

                        var doc = await HandleDocument(document, metadata);

                        Console.WriteLine("- " + document.FilePath.Replace(Path.GetDirectoryName(project.FilePath), "").TrimStart('\\'));

                        project = doc.Project;
                    }

                    Console.WriteLine($"Mutations completed in {0:n2}s ({1}ms)", sw3.ElapsedMilliseconds / 1000.0f, sw3.ElapsedMilliseconds);
                }

                solution = project.Solution;
            }

            var swChanges = Stopwatch.StartNew();
            workspace.TryApplyChanges(solution);

            sw.Stop();

            Console.WriteLine("TryApplyChanges time: {0:n2}s ({1}ms)", (swChanges.ElapsedMilliseconds) / 1000.0f, swChanges.ElapsedMilliseconds);

            Console.WriteLine("Elapsed time: {0:n2}s ({1}ms)", sw.ElapsedMilliseconds / 1000.0f, sw.ElapsedMilliseconds);
        }

        private static void WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            //Console.WriteLine(e.Diagnostic.Message);
        }

        private static bool IsValidDocumentName(string name)
        {
            if (/*name != "VesselVisitItu.cs" ||*/ name.EndsWith(".generated.cs") || name.EndsWith(".AssemblyAttributes.cs") || name.EndsWith(".AssemblyInfo.cs"))
            {
                return false;
            }

            return true;

        }

        private static async Task<Document> HandleDocument(Document document, ProjectMetadata projectMetadata)
        {
            if (projectMetadata.Documents.Any(x => x.FilePath == document.FilePath))
            {
                var syntaxTree = await document.GetSyntaxRootAsync();

                var visitors = Container.GetAllInstances<IVisitor>();

                foreach (var visitor in visitors)
                {
                    syntaxTree = await visitor.Visit(syntaxTree, document, projectMetadata);
                }

                syntaxTree = Formatter.Format(syntaxTree, Container.GetInstance<MSBuildWorkspace>(), Container.GetInstance<OptionSet>());

                return document.WithSyntaxRoot(syntaxTree);
            }

            return document;
        }
    }
}
