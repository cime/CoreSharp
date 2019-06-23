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

            var isAsync = _commandHandlerType.IsAssignableToGenericType(typeof(IAsyncCommandHandler<>)) ||
                          _commandHandlerType.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>));

            var handleMethodInfo =
                _commandHandlerType.GetMethod(isAsync ? "HandleAsync" : "Handle", BindingFlags.Instance | BindingFlags.Public);

            return handleMethodInfo.Invoke(commandHandler, new[] { command });
        }
    }
}
