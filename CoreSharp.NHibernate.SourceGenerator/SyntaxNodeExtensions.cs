﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CoreSharp.NHibernate.SourceGenerator
{
    public static class SyntaxNodeExtensions
    {
        public static bool HasKeyword(this SyntaxNode node, SyntaxKind kind)
        {
            return node.DescendantTokens().SingleOrDefault(x => x.Kind() == kind) != default(SyntaxToken);
        }

        public static bool IsReadonly(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.ReadOnlyKeyword);
        }

        public static bool IsAbstract(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.AbstractKeyword);
        }

        public static bool IsVirtual(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.VirtualKeyword) || HasKeyword(node, SyntaxKind.OverrideKeyword);
        }

        public static bool IsPublic(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.PublicKeyword);
        }

        public static bool IsPrivate(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.PrivateKeyword);
        }

        public static bool IsStatic(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.StaticKeyword);
        }

        public static bool IsPartial(this SyntaxNode node)
        {
            return HasKeyword(node, SyntaxKind.PartialKeyword);
        }

        public static SyntaxToken GetTokenWithKeyword(this SyntaxNode node, SyntaxKind kind)
        {
            return node.DescendantTokens().FirstOrDefault(x => x.Kind() == kind);
        }

        public static string GetIdentifierValue(this SyntaxNode node)
        {
            return node.ChildTokens().FirstOrDefault(x => x.Kind() == SyntaxKind.IdentifierToken).ValueText;
        }
    }
}
