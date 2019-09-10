using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSharp.NHibernate.Generator.Models
{
    internal class TableAttribute
    {
        public string Name { get; }
        public bool View { get; }

        public TableAttribute(AttributeData attributeData)
        {
            foreach (var argument in attributeData.NamedArguments)
            {
                if (argument.Key == "Name")
                {
                    Name = (string)argument.Value.Value;
                }
                else if (argument.Key == "View")
                {
                    View = (bool)argument.Value.Value;
                }
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Name) + ",nq}, Documents: {Documents.Count}, Classes: {Classes.Count}")]
    internal class ProjectMetadata
    {
        public string Name { get; set; }
        public Settings Settings { get; set; }
        public ConcurrentBag<DocumentMetadata> Documents { get; set; } = new ConcurrentBag<DocumentMetadata>();
        public IList<ClassMetadata> Classes => Documents.SelectMany(x => x.Classes).ToList();
        public IList<OverrideClassMetadata> Overrides => Documents.SelectMany(x => x.Overrides).ToList();
    }

    [DebuggerDisplay("{" + nameof(FilePath) + ",nq}")]
    internal class DocumentMetadata
    {
        public string FilePath { get; set; }
        public IList<ClassMetadata> Classes { get; set; }
        public IList<OverrideClassMetadata> Overrides { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Name) + (",nq}, Entity: {" + nameof(EntityType) + ",nq}"))]
    internal class OverrideClassMetadata
    {
        public string Name { get; set; }
        public INamedTypeSymbol Symbol { get; set; }
        public MethodDeclarationSyntax Method { get; set; }
        public string EntityType { get; set; }
        public IList<ExpressionStatementSyntax> ExpressionsStatementSyntaxs => Method.Body.Statements.OfType<ExpressionStatementSyntax>().ToList();
        public IList<string> ExpressionStrings => ExpressionsStatementSyntaxs.Select(x => x.ToFullString()).ToList();
        public IList<OverrideExpression> Expressions => ExpressionsStatementSyntaxs.Select(x => new OverrideExpression(x)).Where(x => x.IsValid).ToList();
    }

    [DebuggerDisplay("{" + nameof(PropertyName) + (",nq}, {" + nameof(Type) + ",nq}"))]
    internal class OverrideExpression
    {
        private static readonly Regex HasRegex = new Regex(@"\.(?<Type>HasMany|HasOne|HasManyToMany)\([\w\s]+=>[\s\w]+\.(?<Property>[\w\d]+)\)([^;]*)", RegexOptions.Compiled);
        private static readonly Regex AsCollectionTypeRegex = new Regex(@"\.(?<Type>AsList|AsSet|AsBag)", RegexOptions.Compiled);
        private static readonly Regex ReferenceRegex = new Regex(@"\.(?<Type>KeyColumn|PropertyRef)\([\w\s]+=>[\s\w]+\.(?<Property>[\w\d]+)\)|\.(?<Type>KeyColumn|PropertyRef)\(?<Property>""([\w\d]+)""\)", RegexOptions.Compiled);

        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceType { get; set; }
        public string AsCollectionType { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(PropertyName) && !string.IsNullOrEmpty(ReferenceName);

        public OverrideExpression(ExpressionStatementSyntax expressionStatementSyntax)
        {
            var expression = expressionStatementSyntax.ToFullString();

            var match = HasRegex.Match(expression);
            if (match.Success)
            {
                PropertyName = match.Groups["Property"].Value;
                PropertyType = match.Groups["Type"].Value;
            }

            match = ReferenceRegex.Match(expression);
            if (match.Success)
            {
                ReferenceName = match.Groups["Property"].Value;
                ReferenceType = match.Groups["Type"].Value;
            }

            match = AsCollectionTypeRegex.Match(expression);
            if (match.Success)
            {
                AsCollectionType = match.Groups["Type"].Value;
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Name) + ",nq}, Properties: {Properties.Count}, Methods: {Methods.Count}")]
    internal class ClassMetadata
    {
        public string Name { get; set; }
        public INamedTypeSymbol Symbol { get; set; }
        public IList<PropertyMetadata> Properties { get; set; }
        public IList<MethodMetadata> Methods { get; set; }
        public TableAttribute TableAttribute { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Name) + ",nq}")]
    internal class PropertyMetadata
    {
        public static readonly string[] CollectionTypes = { "IEnumerable", "IList", "ISet", "HashSet", "List", "ICollection" };

        public string Name { get; set; }
        public IPropertySymbol Symbol { get; set; }
        public INamedTypeSymbol Type { get; set; }
        public bool NameIsTypeName => Name == Type.Name;
        public bool IsCollection => Type.AllInterfaces.Any(x => CollectionTypes.Contains(((INamedTypeSymbol)Type).Name)) && Type.Kind != SymbolKind.ArrayType && Type.Name != "String";
    }

    [DebuggerDisplay("{" + nameof(Name) + ",nq}")]
    internal class MethodMetadata
    {
        public string Name { get; set; }
        public IMethodSymbol Symbol { get; set; }
        public INamedTypeSymbol Type { get; set; }
    }
}
