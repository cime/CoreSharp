using System.Linq;
using Microsoft.CodeAnalysis;

namespace CoreSharp.NHibernate.Generator.Extensions
{
    public static class NamedTypeSymbolExtensions
    {
        public static bool HasMember(this INamedTypeSymbol symbol, string memberName)
        {
            return symbol.MemberNames.Contains(memberName);
        }

        public static bool HasMethod(this INamedTypeSymbol symbol, string memberName)
        {
            return HasMember(symbol, memberName) && symbol.GetMembers(memberName).Any(x => x.Kind == SymbolKind.Method);
        }

        public static bool HasProperty(this INamedTypeSymbol symbol, string memberName)
        {
            return HasMember(symbol, memberName) && symbol.GetMembers(memberName).Any(x => x.Kind == SymbolKind.Property);
        }
    }
}
