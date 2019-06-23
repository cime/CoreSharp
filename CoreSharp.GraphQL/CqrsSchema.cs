using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;
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
        private JsonSerializerSettings _getJsonSerializerSettings;

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
            return new DocumentWriter(Formatting.Indented, GetJsonSerializerSettings()).Write((object) new DocumentExecuter().ExecuteAsync((Action<ExecutionOptions>) (_ =>
            {
                _.Schema = this;
                configure(_);
            })).GetAwaiter().GetResult());
        }

        public virtual async Task<string> ExecuteAsync(Action<ExecutionOptions> configure)
        {
            return new DocumentWriter(Formatting.Indented, GetJsonSerializerSettings()).Write((object) await new DocumentExecuter().ExecuteAsync((Action<ExecutionOptions>) (_ =>
            {
                _.Schema = this;
                configure(_);
            })));
        }
        
        public virtual JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (_getJsonSerializerSettings == null) {
                _getJsonSerializerSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    TypeNameHandling = TypeNameHandling.None,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                };
            }
            
            return _getJsonSerializerSettings;
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
                var descriptionAttribute = commandHandlerType.GetTypeInfo().GetCustomAttribute<DescriptionAttribute>();
                var genericType = commandHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
                                                                                 x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>)));
                var genericArguments = genericType.GetGenericArguments();
                var commandType = genericArguments[0];
                var resultType = genericArguments[1];

                var exposeAttribute = commandType.GetCustomAttribute<ExposeAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                var inputTypeName = commandType.Name;
                var inputObjectType = typeof(InputObjectGraphType<>).MakeGenericType(commandType);
                var inputGqlType = (IInputObjectGraphType)Activator.CreateInstance(inputObjectType);

                inputGqlType.Name = inputTypeName;

                var addFieldMethod = inputGqlType.GetType().GetMethod("AddField");
                var properties = commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var propertyInfo in properties)
                {
                    addFieldMethod.Invoke(inputGqlType, new[]
                    {
                        new FieldType()
                        {
                            Name = propertyInfo.Name.ToCamelCase(),
                            Type = propertyInfo.PropertyType.GetGraphTypeFromType()
                        }
                    });
                }

                IGraphType resultGqlType = null;

                if (!GraphTypeTypeRegistry.Contains(resultType))
                {
                    var resultTypeName = resultType.Name;
                    var returnObjectType = typeof(EntityGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)Activator.CreateInstance(returnObjectType, null);
                    resultGqlType.Name = resultTypeName;
                }

                var queryArgument = new QueryArgument(inputGqlType);
                queryArgument.Name = "command";

                var commandQueryParameters = new List<QueryArgument>()
                {
                    queryArgument
                };

                var mutationName = exposeAttribute.IsUriSet ? exposeAttribute.Uri : new Regex("Command$").Replace(commandType.Name, "");

                if (!Mutation.HasField(mutationName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? GraphTypeTypeRegistry.Get(resultType) : null,
                        ResolvedType = resultGqlType,
                        Resolver = new CommandResolver(_container, commandHandlerType, commandType),
                        Name = GetNormalizedFieldName(mutationName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(commandQueryParameters)
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
                var descriptionAttribute = queryHandlerType.GetTypeInfo().GetCustomAttribute<DescriptionAttribute>();
                var genericType = queryHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(IQueryHandler<,>)) || 
                                                                               x.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>)));
                var genericArguments = genericType.GetGenericArguments();
                var queryType = genericArguments[0];
                var resultType = genericArguments[1];

                var exposeAttribute = queryType.GetCustomAttribute<ExposeAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                var inputTypeName = queryType.Name;
                var inputObjectType = typeof(InputObjectGraphType<>).MakeGenericType(queryType);
                var inputGqlType = (IInputObjectGraphType)Activator.CreateInstance(inputObjectType);

                inputGqlType.Name = inputTypeName;

                var addFieldMethod = inputGqlType.GetType().GetMethod("AddField");
                var properties = queryType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                //TODO autoregistering input type
                foreach (var propertyInfo in properties)
                {
                    addFieldMethod.Invoke(inputGqlType, new[]
                    {
                        new FieldType()
                        {
                            Name = GetNormalizedFieldName(propertyInfo.Name),
                            Type = propertyInfo.PropertyType.GetGraphTypeFromType()
                        }
                    });
                }

                IGraphType resultGqlType = null;

                if (!GraphTypeTypeRegistry.Contains(resultType))
                {
                    var resultTypeName = resultType.Name;
                    var returnObjectType = typeof(EntityGraphType<>).MakeGenericType(resultType);
                    resultGqlType = (IGraphType)Activator.CreateInstance(returnObjectType, null);
                    resultGqlType.Name = resultTypeName;
                }

                var queryArgument = new QueryArgument(inputGqlType);
                queryArgument.Name = "query";

                var queryQueryParameters = new List<QueryArgument>()
                {
                    queryArgument
                };

                var queryName = exposeAttribute.IsUriSet ? exposeAttribute.Uri : new Regex("Query").Replace(queryType.Name, "");

                if (!Query.HasField(queryName))
                {
                    var type = new FieldType
                    {
                        Type = resultGqlType == null ? GraphTypeTypeRegistry.Get(resultType) : null,
                        ResolvedType = resultGqlType,
                        Resolver = new QueryResolver(_container, queryHandlerType, queryType),
                        Name = GetNormalizedFieldName(queryName),
                        Description = descriptionAttribute?.Description,
                        Arguments = new QueryArguments(queryQueryParameters)
                    };

                    Query.AddField(type);
                }
            }
        }

        protected virtual string GetNormalizedFieldName(string value)
        {
            var parts = value.Split(new [] { '/' })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => global::GraphQL.StringExtensions.ToPascalCase(x));

            return string.Join("", parts).ToCamelCase();
        }
    }
}
