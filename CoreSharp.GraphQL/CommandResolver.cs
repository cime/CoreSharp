using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Command;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class CommandResolver : IFieldResolver
    {
        private readonly Container _container;
        private readonly Type _commandHandlerType;
        private readonly Type _commandType;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public CommandResolver(Container container, Type commandHandlerType, Type commandType,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _container = container;
            _commandHandlerType = commandHandlerType;
            _commandType = commandType;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public object Resolve(IResolveFieldContext context)
        {
            var commandHandler = _container.GetInstance(_commandHandlerType);
            var variableValue = context.Arguments?.ContainsKey("command") == true ? JsonConvert.SerializeObject(context.Arguments["command"], _jsonSerializerSettings) : "{}";
            var command = JsonConvert.DeserializeObject(variableValue, _commandType, _jsonSerializerSettings);

            if (command != null)
            {
                var contextProperties = command.GetType().GetProperties().Where(x => typeof(IResolveFieldContext).IsAssignableFrom(x.PropertyType) && x.CanWrite).ToList();

                foreach (var contextProperty in contextProperties)
                {
                    contextProperty.SetValue(command, context);
                }
            }

            var isAsync = _commandHandlerType.IsAssignableToGenericType(typeof(IAsyncCommandHandler<>)) ||
                          _commandHandlerType.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>));

            var handleMethodInfo =
                _commandHandlerType.GetMethod(isAsync ? "HandleAsync" : "Handle", BindingFlags.Instance | BindingFlags.Public);

            return handleMethodInfo.Invoke(commandHandler, new[] { command });
        }
    }
}
