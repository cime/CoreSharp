using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using CoreSharp.Common.Command;
using CoreSharp.Common.Query;
using GraphQL;
using GraphQL.Http;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using Newtonsoft.Json;
using Container = SimpleInjector.Container;

namespace CoreSharp.GraphQL
{
    public class CqrsSchema : Schema
    {
        private readonly Container _container;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            TypeNameHandling = TypeNameHandling.None,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

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

        public string Execute(Action<ExecutionOptions> configure)
        {
            return new DocumentWriter(Formatting.Indented, _jsonSerializerSettings).Write((object) new DocumentExecuter().ExecuteAsync((Action<ExecutionOptions>) (_ =>
            {
                _.Schema = this;
                configure(_);
            })).GetAwaiter().GetResult());
        }

        public async Task<string> ExecuteAsync(Action<ExecutionOptions> configure)
        {
            return new DocumentWriter(Formatting.Indented, _jsonSerializerSettings).Write((object) await new DocumentExecuter().ExecuteAsync((Action<ExecutionOptions>) (_ =>
            {
                _.Schema = this;
                configure(_);
            })));
        }

        public void RegisterCommands()
        {
            var registrations = _container.GetRootRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var commandHandlerTypes = registeredTypes.Where(x =>
                x.GetTypeInfo().IsAssignableToGenericType(typeof(ICommandHandler<,>))).ToList();


            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var descriptionAttribute = commandHandlerType.GetTypeInfo().GetCustomAttribute<DescriptionAttribute>();
                var genericType = commandHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(ICommandHandler<,>)));
                var genericArguments = genericType.GetGenericArguments();
                var commandType = genericArguments[0];
                var resultType = genericArguments[1];

                var exposeAttribute = commandType.GetCustomAttribute<ExposeAttribute>();

                if (exposeAttribute == null)
                {
                    continue;
                }

                //
                // For each command here I would like to create a mutation that has 1 input of type commandType and returns the resultType
                // Example command can be found inside Commands/TestCommand.cs
                //
                // Each command can be resolved using: var result = (new "commandHandlerType"()).Handle("command instance");
                //

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
                    var returnObjectType = typeof(AutoRegisteringObjectGraphType<>).MakeGenericType(resultType);
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

        public void RegisterQueries()
        {
            var registrations = _container.GetRootRegistrations();
            var registeredTypes = registrations.Select(x => x.Registration.ImplementationType).Distinct().ToList();
            var queryHandlerTypes = registeredTypes.Where(x =>
                x.GetTypeInfo().IsAssignableToGenericType(typeof(IQueryHandler<,>))).ToList();


            foreach (var queryHandlerType in queryHandlerTypes)
            {
                var descriptionAttribute = queryHandlerType.GetTypeInfo().GetCustomAttribute<DescriptionAttribute>();
                var genericType = queryHandlerType.GetInterfaces().Single(x => x.GetTypeInfo().IsAssignableToGenericType(typeof(IQueryHandler<,>)));
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
                    var returnObjectType = typeof(AutoRegisteringObjectGraphType<>).MakeGenericType(resultType);
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

        private static string GetNormalizedFieldName(string value)
        {
            var parts = value.Split(new [] { '/' })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => global::GraphQL.StringExtensions.ToPascalCase(x));

            return string.Join("", parts).ToCamelCase();
        }

        public class CommandResolver : IFieldResolver
        {
            private readonly Container _container;
            private readonly Type _commandHandlerType;
            private readonly Type _commandType;

            public CommandResolver(Container container, Type commandHandlerType, Type commandType)
            {
                _container = container;
                _commandHandlerType = commandHandlerType;
                _commandType = commandType;
            }

            public object Resolve(ResolveFieldContext context)
            {
                var commandHandler = _container.GetInstance(_commandHandlerType);
                // TODO: find a better way to deserialize variable command
                var variableValue = context.Variables.SingleOrDefault(x => x.Name == "command")?.Value;
                var command =
                    JsonConvert.DeserializeObject(JsonConvert.SerializeObject(variableValue), _commandType);

                var handleMethodInfo =
                    _commandHandlerType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public);

                return handleMethodInfo.Invoke(commandHandler, new[] { command });
            }
        }

        public class QueryResolver : IFieldResolver
        {
            private readonly Container _container;
            private readonly Type _queryHandlerType;
            private readonly Type _queryType;

            public QueryResolver(Container container, Type queryHandlerType, Type queryType)
            {
                _container = container;
                _queryHandlerType = queryHandlerType;
                _queryType = queryType;
            }

            public object Resolve(ResolveFieldContext context)
            {
                var queryHandler = _container.GetInstance(_queryHandlerType);
                // TODO: find a better way to deserialize variable query
                var variableValue = context.Variables.SingleOrDefault(x => x.Name == "query")?.Value;
                var query =
                    JsonConvert.DeserializeObject(JsonConvert.SerializeObject(variableValue), _queryType);

                var handleMethodInfo =
                    _queryHandlerType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public);

                return handleMethodInfo.Invoke(queryHandler, new[] { query });
            }
        }
    }
}
