using System;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreSharp.Common.Attributes;

namespace CoreSharp.Cqrs.Resolver
{
    public static class CqrsExposeUtil
    {

        private static readonly Regex _pathValidationRegex = new Regex("^[a-z0-9/]{3,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _commandNameSuffixRegex = new Regex("(?:AsyncCommand|CommandAsync|Command)$", RegexOptions.Compiled);
        private static readonly Regex _queryNameSuffixRegex = new Regex("(?:AsyncQuery|QueryAsync|Query)$", RegexOptions.Compiled);
        private static readonly Regex _queryNamePrefixRegex = new Regex("^Get", RegexOptions.Compiled);

        public static string GetCommandKey(Type type)
        {
            var exposeAttribute = type.GetCustomAttribute<ExposeAttribute>();
            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/').TrimStart('/') : GetCommandNameFromType(type);

            if (!_pathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for command '{type.Namespace}.{type.Name}'");
            }

            return key.ToLowerFirstChar();
        }

        public static string GetQueryKey(Type type)
        {
            var exposeAttribute = type.GetCustomAttribute<ExposeAttribute>();

            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/').TrimStart('/') : GetQueryNameFromType(type);

            if (!_pathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for query '{type.Namespace}.{type.Name}'");
            }

            return key.ToLowerFirstChar();
        }

        public static string GetFormatter(Type type)
        {
            var formatter = type.GetCustomAttribute<ExposeAttribute>()?.Formatter;
            return !string.IsNullOrWhiteSpace(formatter) ? formatter : null;
        }

        private static string GetCommandNameFromType(Type type)
        {
            return _commandNameSuffixRegex.Replace(type.Name, string.Empty);
        }

        private static string GetQueryNameFromType(Type type)
        {
            var queryName = _queryNameSuffixRegex.Replace(type.Name, string.Empty);

            return _queryNamePrefixRegex.Replace(queryName, String.Empty);
        }

    }
}
