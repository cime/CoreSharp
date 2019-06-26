using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;
using CoreSharp.GraphQL.Attributes;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Utilities;
using Newtonsoft.Json;
using Container = SimpleInjector.Container;

namespace CoreSharp.GraphQL
{
    public abstract class CqrsSchema : Schema
    {
        private readonly Container _container;
        private JsonSerializerSettings? _jsonSerializerSettings;

        public CqrsSchema(Container container)
        {
            _container = container;

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

        public virtual string Execute(Action<ExecutionOptions> configure)
        {
            return new DocumentWriter(Formatting.Indented, GetJsonSerializerSettings()).Write((object) new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = this;
                configure(_);
            }).GetAwaiter().GetResult());
        }

        public virtual async Task<string> ExecuteAsync(Action<ExecutionOptions> configure)
        {
            return new DocumentWriter(Formatting.Indented, GetJsonSerializerSettings()).Write((object) await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = this;
                configure(_);
            }));
        }

        public virtual JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (_jsonSerializerSettings == null) {
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
            var registrations = _container.GetRootRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var commandHandlerTypes = registeredTypes.Where(x =>
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
                var resultType = genericArguments[1];

                var descriptionAttribute = commandType.GetCustomAttribute<DescriptionAttribute>();
                var exposeAttribute = commandType.GetCustomAttribute<ExposeGraphQLAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                IInputObjectGraphType inputGqlType = null;

                var properties = commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                if (properties.Any())
                {
                    var inputObjectType = typeof(InputObjectGraphType<>).MakeGenericType(commandType);
                    inputGqlType = (IInputObjectGraphType)Activator.CreateInstance(inputObjectType);
                    inputGqlType.Name = commandType.Name;
                    inputGqlType.Description = descriptionAttribute?.Description;

                    var addFieldMethod = inputGqlType.GetType().GetMethod("AddField");

                    foreach (var propertyInfo in properties)
                    {
                        addFieldMethod.Invoke(inputGqlType,
                            new[]
                            {
                                new FieldType()
                                {
                                    Name = propertyInfo.Name.ToCamelCase(),
                                    Type = propertyInfo.PropertyType.GetGraphTypeFromType(),
                                    Description = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                }
                            });
                    }
                }

                IGraphType? resultGqlType = null;

                if (!GraphTypeTypeRegistry.Contains(resultType))
                {
                    var resultTypeName = resultType.Name;
                    var returnObjectType = typeof(EntityGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)Activator.CreateInstance(returnObjectType, null);
                    resultGqlType.Name = resultTypeName;
                }

                var arguments = new List<QueryArgument>();

                if (inputGqlType != null)
                {
                    var argument = new QueryArgument(inputGqlType);
                    argument.Name = "command";
                    arguments.Add(argument);
                }

                var mutationName = exposeAttribute.IsFieldNameSet ? exposeAttribute.FieldName : new Regex("Command$").Replace(commandType.Name, "");

                if (!Mutation.HasField(mutationName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? GraphTypeTypeRegistry.Get(resultType) : null,
                        ResolvedType = resultGqlType,
                        Resolver = new CommandResolver(_container, commandHandlerType, commandType, GetJsonSerializerSettings()),
                        Name = GetNormalizedFieldName(mutationName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(arguments)
                    };

                    Mutation.AddField(type);
                }
            }
        }

        public virtual void RegisterQueriesFromContainer()
        {
            var registrations = _container.GetRootRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var queryHandlerTypes = registeredTypes.Where(x =>
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
                var resultType = genericArguments[1];

                var descriptionAttribute = queryType.GetCustomAttribute<DescriptionAttribute>();
                var exposeAttribute = queryType.GetCustomAttribute<ExposeGraphQLAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                var properties = queryType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                IInputObjectGraphType inputGqlType = null;

                if (properties.Any())
                {
                    var inputObjectType = typeof(InputObjectGraphType<>).MakeGenericType(queryType);
                    inputGqlType = (IInputObjectGraphType)Activator.CreateInstance(inputObjectType);

                    inputGqlType.Name = queryType.Name;
                    inputGqlType.Description = descriptionAttribute?.Description;

                    var addFieldMethod = inputGqlType.GetType().GetMethod("AddField");

                    foreach (var propertyInfo in properties)
                    {
                        addFieldMethod.Invoke(inputGqlType, new[]
                        {
                            new FieldType()
                            {
                                Name = GetNormalizedFieldName(propertyInfo.Name),
                                Type = propertyInfo.PropertyType.GetGraphTypeFromType(),
                                Description = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                            }
                        });
                    }
                }

                IGraphType? resultGqlType = null;

                if (!GraphTypeTypeRegistry.Contains(resultType))
                {
                    var resultTypeName = resultType.Name;
                    var returnObjectType = typeof(EntityGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)Activator.CreateInstance(returnObjectType, null);
                    resultGqlType.Name = resultTypeName;
                }

                var arguments = new List<QueryArgument>();

                if (inputGqlType != null)
                {
                    var argument = new QueryArgument(inputGqlType);
                    argument.Name = "query";
                    arguments.Add(argument);
                }

                var queryName = exposeAttribute.IsFieldNameSet ? exposeAttribute.FieldName : new Regex("Query").Replace(queryType.Name, "");

                if (!Query.HasField(queryName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? GraphTypeTypeRegistry.Get(resultType) : null,
                        ResolvedType = resultGqlType,
                        Resolver = new QueryResolver(_container, queryHandlerType, queryType, GetJsonSerializerSettings()),
                        Name = GetNormalizedFieldName(queryName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(arguments)
                    };

                    Query.AddField(type);
                }
            }
        }

        protected virtual string? GetNormalizedFieldName(string? value)
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
    }
}
