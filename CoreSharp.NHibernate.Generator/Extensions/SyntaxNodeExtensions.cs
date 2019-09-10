using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable once CheckNamespace
namespace Microsoft.CodeAnalysis
{
    internal static class SyntaxNodeExtensions
    {
        internal static bool HasKeyword(this SyntaxNode node, SyntaxKind kind)
        {
            return node.DescendantTokens().SingleOrDefault(x => x.Kind() == kind) != default(SyntaxToken);
        }

        internal static bool IsReadonly(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.ReadOnlyKeyword);
        }

        internal static bool IsVirtual(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.VirtualKeyword);
        }

        internal static bool IsPublic(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.PublicKeyword);
        }

        internal static bool IsProtected(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.ProtectedKeyword);
        }

        internal static bool IsPrivate(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.PrivateKeyword);
        }

        internal static SyntaxToken GetTokenWithKeyword(this SyntaxNode node, SyntaxKind kind)
        {
            return node.DescendantTokens().FirstOrDefault(x => x.Kind() == kind);
        }

        internal static string GetIdentifierValue(this SyntaxNode node)
        {
            return node.ChildTokens().FirstOrDefault(x => x.Kind() == SyntaxKind.IdentifierToken).ValueText;
        }

        internal static SyntaxList<UsingDirectiveSyntax> SortUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives, bool placeSystemNamespaceFirst = false)
        {
            return SyntaxFactory.List(
                usingDirectives
                    .OrderBy(x => x.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) ? 1 : x.Alias == null ? 0 : 2)
                    //.ThenBy(x => x.Name.ToString().StartsWith("System") ? x.Name.ToString().Count(c => c == '.') : 1000)
                    .ThenBy(x => x.Alias?.ToString())
                    .ThenByDescending(x => placeSystemNamespaceFirst && x.Name.ToString().StartsWith(nameof(System)))
                    .ThenBy(x => x.Name.ToString()));
        }

        /// <summary>
        /// Generates the type syntax.
        /// </summary>
        internal static TypeSyntax AsTypeSyntax(this Type type, string genericParameter)
        {
            string name = type.Name.Replace('+', '.');

            if (type.IsGenericType)
            {
                // Get the C# representation of the generic type minus its type arguments.
                name = name.Substring(0, name.IndexOf("`"));

                // Generate the name of the generic type.
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier(name),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(new [] { SyntaxFactory.ParseTypeName(genericParameter) }))
                );
            }
            else
                return SyntaxFactory.ParseTypeName(name);
        }
    }
}
