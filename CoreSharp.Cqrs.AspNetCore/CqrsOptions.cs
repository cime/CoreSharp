using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreSharp.Common.Attributes;
using CoreSharp.Common.Command;
using CoreSharp.Common.Query;
using CoreSharp.NHibernate.Json;
using Newtonsoft.Json;
using NHibernate;
using SimpleInjector;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class CqrsOptions : ICqrsOptions
    {
        private readonly Container _container;
        private readonly Regex _pathValidationRegex = new Regex("^[a-z0-9/]{3,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string CommandsPath { get; set; } = "/api/command/";
        public string QueriesPath { get; set; } = "/api/query/";

        public CqrsOptions(Container container)
        {
            _container = container;
        }

        public string GetCommandKey(CommandInfo type)
        {
            var exposeAttribute = type.CommandType.GetCustomAttribute<ExposeAttribute>();

            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/') : type.CommandType.Name.Replace("Command", string.Empty);

            if (!_pathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for command '{type.CommandType.Namespace}.{type.CommandType.Name}'");
            }

            return key;
        }

        public string GetQueryKey(QueryInfo info)
        {
            var exposeAttribute = info.QueryType.GetCustomAttribute<ExposeAttribute>();

            var key = exposeAttribute.IsUriSet ? exposeAttribute.Uri.Replace("//", "/").TrimEnd('/') : info.QueryType.Name.Replace("Query", string.Empty);

            if (!_pathValidationRegex.IsMatch(key))
            {
                throw new FormatException($"Invalid path '{key}' for query '{info.QueryType.Namespace}.{info.QueryType.Name}'");
            }

            return key;
        }

        public string GetQueryPath(string path)
        {
            return path.Substring(QueriesPath.Length, path.Length - QueriesPath.Length);
        }

        public string GetCommandPath(string path)
        {
            return path.Substring(CommandsPath.Length, path.Length - CommandsPath.Length);
        }


        public IEnumerable<QueryInfo> GetQueryTypes()
        {
            return _container.GetRootRegistrations()
                .Select(x => x.Registration.ImplementationType)
                .Distinct()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    (
                        t.IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                        t.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>))
                    )
                )
                .SelectMany(x =>
                {
                    return x.GetInterfaces().Where(i => i.IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                                                        i.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>)))
                        .Select(i => i.GetGenericArguments().First());
                })
                .Where(x => x.GetCustomAttribute<ExposeAttribute>() != null)
                .Select(t => new QueryInfo(t));
        }

        public IEnumerable<CommandInfo> GetCommandTypes()
        {
            return _container.GetRootRegistrations()
                .Select(x => x.Registration.ImplementationType)
                .Distinct()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    (
                        t.IsAssignableToGenericType(typeof(ICommandHandler<>)) ||
                        t.IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
                        t.IsAssignableToGenericType(typeof(IAsyncCommandHandler<>)) ||
                        t.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>))
                    )
                )
                .SelectMany(x =>
                {
                    return x.GetInterfaces().Where(i => i.IsAssignableToGenericType(typeof(ICommandHandler<>)) ||
                                                        i.IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
                                                        i.IsAssignableToGenericType(typeof(IAsyncCommandHandler<>)) ||
                                                        i.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>)))
                        .Select(i => i.GetGenericArguments().First());
                })
                .Where(x => x.GetCustomAttribute<ExposeAttribute>() != null)
                .Select(t => new CommandInfo(t));
        }

        public JsonSerializerSettings GetJsonSerializerSettings()
        {
            var contractResolver = _container.GetInstance<NHibernateContractResolver>();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = contractResolver,
                /* Error handling is not needed anymore. NHibernateContractResolver will take care of non initialized properties*/
                //FIX: Still errors occurs
                Error = (sender, args) =>
                {
                    // When the NHibernate session is closed, NH proxies throw LazyInitializationException when
                    // the serializer tries to access them.  We want to ignore those exceptions.
                    var error = args.ErrorContext.Error;
                    if (error is LazyInitializationException || error is ObjectDisposedException)
                        args.ErrorContext.Handled = true;
                }
            };

            if (!settings.Converters.Any(o => o is NHibernateProxyJsonConverter))
            {
                settings.Converters.Add(new NHibernateProxyJsonConverter());
            }

            return settings;
        }
    }
}
