using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreSharp.Common.Attributes;

namespace CoreSharp.Cqrs.AspNetCore.Options
{
    public abstract class AbstractCqrsOptions : ICqrsOptions
    {
        protected readonly Regex PathValidationRegex = new Regex("^[a-z0-9/]{3,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected readonly Regex CommandNameSuffixRegex = new Regex("(?:AsyncCommand|CommandAsync|Command)$", RegexOptions.Compiled);
        protected readonly Regex QueryNameSuffixRegex = new Regex("(?:AsyncQuery|QueryAsync|Query)$", RegexOptions.Compiled);
        protected readonly Regex QueryNamePrefixRegex = new Regex("^Get", RegexOptions.Compiled);

        public string CommandsPath { get; set; } = "/api/command/";
        public string QueriesPath { get; set; } = "/api/query/";

        public string GetCommandKey(CommandInfo info)
        {
            var exposeAttribute = info.CommandType.GetCustomAttribute<ExposeAttribute>();
            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/') : GetCommandNameFromType(info.CommandType);

            if (!PathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for command '{info.CommandType.Namespace}.{info.CommandType.Name}'");
            }

            return key;
        }

        public string GetQueryKey(QueryInfo info)
        {
            var exposeAttribute = info.QueryType.GetCustomAttribute<ExposeAttribute>();

            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/') : GetQueryNameFromType(info.QueryType);

            if (!PathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for query '{info.QueryType.Namespace}.{info.QueryType.Name}'");
            }

            return key;
        }

        private string GetCommandNameFromType(Type type)
        {
            return CommandNameSuffixRegex.Replace(type.Name, string.Empty);
        }

        private string GetQueryNameFromType(Type type)
        {
            var queryName = QueryNameSuffixRegex.Replace(type.Name, string.Empty);

            return QueryNamePrefixRegex.Replace(queryName, String.Empty);
        }

        public string GetQueryPath(string path)
        {
            return path.Substring(QueriesPath.Length, path.Length - QueriesPath.Length);
        }

        public string GetCommandPath(string path)
        {
            return path.Substring(CommandsPath.Length, path.Length - CommandsPath.Length);
        }

        public abstract IEnumerable<CommandInfo> GetCommandTypes();
        public abstract IEnumerable<QueryInfo> GetQueryTypes();
        public abstract object GetInstance(Type type);
    }
}
