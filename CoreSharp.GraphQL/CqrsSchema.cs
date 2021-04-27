using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;
using CoreSharp.GraphQL.Attributes;
using CoreSharp.GraphQL.Configuration;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using Newtonsoft.Json;
using Container = SimpleInjector.Container;

namespace CoreSharp.GraphQL
{
    public abstract class CqrsSchema : Schema
    {
        private static readonly ConcurrentDictionary<Type, IGraphType> _typeCache = new ConcurrentDictionary<Type, IGraphType>();

        private readonly Regex _commandNameSuffixRegex = new Regex("(?:AsyncCommand|CommandAsync|Command)$", RegexOptions.Compiled);
        private readonly Regex _queryNameSuffixRegex = new Regex("(?:AsyncQuery|QueryAsync|Query)$", RegexOptions.Compiled);
        private readonly Regex _queryNamePrefixRegex = new Regex("^Get", RegexOptions.Compiled);

        private readonly Container _container;
        private readonly IGraphQLConfiguration _configuration;
        private JsonSerializerSettings _jsonSerializerSettings;

        public virtual bool AllowNullQuery => false;
        public virtual bool AllowNullCommand => false;

        public CqrsSchema(Container container, IGraphQLConfiguration configuration, IServiceProvider provider) : base(provider)
        {
            _container = container;
            _configuration = configuration;

            Query = new ObjectGraphType()
            {
                Name = "Query",
                Description = "Root query"
            };
            Mutation = new ObjectGraphType()
            {
                Name = "Mutation",
                Description = "Root mutation"
            };
        }

        public virtual JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    TypeNameHandling = TypeNameHandling.None,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                };
            }

            return _jsonSerializerSettings;
        }

        public virtual void RegisterCommandsFromContainer()
        {
            if (!_container.IsLocked)
            {
                throw new InvalidOperationException("Container is not Locked");
            }

            var registrations = _container.GetCurrentRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var commandHandlerTypes = registeredTypes
                .Where(x => x.GetCustomAttribute<DecoratorAttribute>() == null)
                .Where(x =>
            x.GetTypeInfo().IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
            x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>))).ToList();

            RegisterCommands(commandHandlerTypes);
        }

        public virtual void RegisterCommands(IEnumerable<Type> commandHandlerTypes)
        {
            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var genericType = commandHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
                                                                                 x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>)));
                var genericArguments = genericType.GetGenericArguments();
                var commandType = genericArguments[0];
                var queryResultType = genericArguments[1];
                var resultType = genericArguments[1];

                if (commandType.GetCustomAttribute<ExposeGraphQLAttribute>() == null)
                {
                    continue;
                }

                var isCollection = resultType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(resultType);

                if (isCollection)
                {
                    resultType = resultType.GetGenericArguments()[0];
                }

                var descriptionAttribute = commandType.GetCustomAttribute<DescriptionAttribute>();
                var exposeAttribute = commandType.GetCustomAttribute<ExposeGraphQLAttribute>();
                var authorizeAttribute = commandType.GetCustomAttribute<AuthorizeAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                IInputObjectGraphType inputGqlType = null;

                var properties = commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => !typeof(IResolveFieldContext).IsAssignableFrom(x.PropertyType)).ToList();

                if (properties.Any())
                {
                    var inputObjectType = typeof(AutoInputGraphType<>).MakeGenericType(commandType);
                    inputGqlType = (IInputObjectGraphType)_container.GetInstance(inputObjectType);
                    inputGqlType.Description = descriptionAttribute?.Description;
                }

                IGraphType resultGqlType = null;

                if (!_typeCache.ContainsKey(queryResultType) && !BuiltInTypeMappings.Any(x => x.clrType == resultType) && !TypeMappings.Any(x => x.clrType == resultType))
                {
                    var resultTypeName = resultType.Name;
                    var returnObjectType = typeof(AutoObjectGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)_container.GetInstance(returnObjectType);
                    resultGqlType.Name = resultTypeName;

                    ListGraphType listGqlType = null;
                    if (isCollection)
                    {
                        var name = resultGqlType.Name;
                        listGqlType = (ListGraphType) Activator.CreateInstance(typeof(ListGraphType<>).MakeGenericType(returnObjectType), null);
                        listGqlType.ResolvedType = resultGqlType;
                        resultGqlType = (IGraphType) listGqlType;
                        // resultGqlType.Name = "ListOf" + name;
                    }

                    RegisterTypeMapping(resultType, returnObjectType);
                }
                else if (_typeCache.ContainsKey(queryResultType))
                {
                    resultGqlType = _typeCache[queryResultType];
                }

                var arguments = new List<QueryArgument>();

                if (inputGqlType != null)
                {
                    var argument = AllowNullCommand ? new QueryArgument(inputGqlType) : new QueryArgument(new NonNullGraphType(inputGqlType));
                    argument.Name = "command";
                    arguments.Add(argument);
                }

                var mutationName = exposeAttribute.IsFieldNameSet ? exposeAttribute.FieldName : _commandNameSuffixRegex.Replace(commandType.Name, string.Empty);

                if (!Mutation.HasField(mutationName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? BuiltInTypeMappings.Where(x => x.clrType == resultType).Select(x => x.graphType).SingleOrDefault() ?? TypeMappings.Where(x => x.clrType == resultType).Select(x => x.graphType).SingleOrDefault() : null,
                        ResolvedType = resultGqlType,
                        Resolver = new CommandResolver(_container, commandHandlerType, commandType, resultType, GetJsonSerializerSettings()),
                        Name = GetNormalizedFieldName(mutationName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(arguments)
                    };
                    type.Metadata[GraphQLExtensions.PermissionsKey] = authorizeAttribute?.Permissions?.ToList();

                    Mutation.AddField(type);
                }
            }
        }

        public virtual void RegisterQueriesFromContainer()
        {
            if (!_container.IsLocked)
            {
                throw new InvalidOperationException("Container is not Locked");
            }

            var registrations = _container.GetCurrentRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var queryHandlerTypes = registeredTypes
                .Where(x => x.GetCustomAttribute<DecoratorAttribute>() == null)
                .Where(x =>
                    x.GetTypeInfo().IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                    x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>))).ToList();

            RegisterQueries(queryHandlerTypes);
        }

        public virtual void RegisterQueries(IEnumerable<Type> queryHandlerTypes)
        {
            foreach (var queryHandlerType in queryHandlerTypes)
            {
                var genericType = queryHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                                                                               x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>)));
                var genericArguments = genericType.GetGenericArguments();
                var queryType = genericArguments[0];
                var queryResultType = genericArguments[1];
                var resultType = genericArguments[1];

                if (queryType.GetCustomAttribute<ExposeGraphQLAttribute>() == null)
                {
                    continue;
                }

                var isCollection = resultType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(resultType);

                if (isCollection)
                {
                    resultType = resultType.GetGenericArguments()[0];
                }

                var descriptionAttribute = queryType.GetCustomAttribute<DescriptionAttribute>();
                var exposeAttribute = queryType.GetCustomAttribute<ExposeGraphQLAttribute>();
                var authorizeAttribute = queryType.GetCustomAttribute<AuthorizeAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                var properties = queryType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => !typeof(IResolveFieldContext).IsAssignableFrom(x.PropertyType));
                IInputObjectGraphType inputGqlType = null;

                if (properties.Any())
                {
                    var inputObjectType = typeof(AutoInputGraphType<>).MakeGenericType(queryType);
                    inputGqlType = (IInputObjectGraphType)_container.GetInstance(inputObjectType);
                    inputGqlType.Description = descriptionAttribute?.Description;
                }

                IGraphType resultGqlType = null;

                if (!_typeCache.ContainsKey(queryResultType) && !BuiltInTypeMappings.Any(x => x.clrType == resultType) && !TypeMappings.Any(x => x.clrType == resultType))
                {
                    var resultTypeName = GetTypeName(resultType);
                    var returnObjectType = typeof(AutoObjectGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)_container.GetInstance(returnObjectType);
                    resultGqlType.Name = resultTypeName;

                    //TODO: refactor
                    ListGraphType listGqlType = null;
                    if (isCollection)
                    {
                        var name = resultGqlType.Name;
                        listGqlType = (ListGraphType) Activator.CreateInstance(typeof(ListGraphType<>).MakeGenericType(returnObjectType), null);
                        listGqlType.ResolvedType = resultGqlType;
                        resultGqlType = (IGraphType) listGqlType;
                        // resultGqlType.Name = "ListOf" + name;
                    }

                    RegisterTypeMapping(resultType, returnObjectType);
                    _typeCache.TryAdd(queryResultType, resultGqlType);
                }
                else if (_typeCache.ContainsKey(queryResultType))
                {
                    resultGqlType = _typeCache[queryResultType];
                }

                var arguments = new List<QueryArgument>();

                if (inputGqlType != null)
                {
                    var argument = AllowNullQuery ? new QueryArgument(inputGqlType) : new QueryArgument(new NonNullGraphType(inputGqlType));
                    argument.Name = "query";
                    arguments.Add(argument);
                }

                var queryName = exposeAttribute.IsFieldNameSet ? exposeAttribute.FieldName : _queryNamePrefixRegex.Replace(_queryNameSuffixRegex.Replace(queryType.Name, string.Empty), string.Empty);

                if (!Query.HasField(queryName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? BuiltInTypeMappings.Where(x => x.clrType == resultType).Select(x => x.graphType).SingleOrDefault() ?? TypeMappings.Where(x => x.clrType == resultType).Select(x => x.graphType).SingleOrDefault() : null,
                        ResolvedType = resultGqlType,
                        Resolver = new QueryResolver(_container, queryHandlerType, queryType, resultType, GetJsonSerializerSettings()),
                        Name = GetNormalizedFieldName(queryName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(arguments),
                    };
                    type.Metadata[GraphQLExtensions.PermissionsKey] = authorizeAttribute?.Permissions?.ToList();

                    Query.AddField(type);
                }
            }
        }

        protected virtual string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeName = type.Name.Remove(type.Name.IndexOf('`'));
                var genericParametersName = type.GetGenericArguments().Select(x => x.Name).ToList();

                return genericTypeName + string.Join("", genericParametersName);
            }

            return type.Name;
        }

        protected virtual string GetNormalizedFieldName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var parts = value.Split(new [] { '/' })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => global::GraphQL.StringExtensions.ToPascalCase(x));

            return string.Join("", parts).ToCamelCase();
        }

        protected virtual bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute))) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullAttribute))) return false;

            if (!propertyInfo.PropertyType.IsValueType) return true;

            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
