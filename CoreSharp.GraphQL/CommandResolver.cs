using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CoreSharp.Cqrs.Command;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class CommandResolver : IFieldResolver
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object, IResolveFieldContext, object>> Cache = new ConcurrentDictionary<Type, Func<object, object, IResolveFieldContext, object>>();

        private readonly Container _container;
        private readonly Type _commandHandlerType;
        private readonly Type _commandType;
        private readonly Type _resultType;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public CommandResolver(Container container, Type commandHandlerType, Type commandType, Type resultType,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _container = container;
            _commandHandlerType = commandHandlerType;
            _commandType = commandType;
            _resultType = resultType;
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
            
            var handleMethodInfo = Cache.GetOrAdd(_commandHandlerType, (type) =>
            {
                var p1 = Expression.Parameter(typeof(object), "commandHandler");
                var p2 = Expression.Parameter(typeof(object), "command");
                var p3 = Expression.Parameter(typeof(IResolveFieldContext), "context");

                var isAsync = type.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>));

                MethodCallExpression call;
                if (isAsync)
                {
                    var mi = _commandHandlerType.GetMethod(nameof(IAsyncCommandHandler<IAsyncCommand<object>, object>.HandleAsync), BindingFlags.Instance | BindingFlags.Public, null, new []{ _commandType, typeof(CancellationToken) }, null);
                    var cn = Expression.Property(p3, "CancellationToken");

                    call = Expression.Call(Expression.Convert(p1, _commandHandlerType), mi, Expression.Convert(p2, _commandType), cn);
                }
                else
                {
                    var mi = _commandHandlerType.GetMethod(nameof(ICommandHandler<ICommand<object>, object>.Handle), BindingFlags.Instance | BindingFlags.Public, null, new []{ _commandType }, null);
                    call = Expression.Call(Expression.Convert(p1, _commandHandlerType), mi, Expression.Convert(p2, _commandType));
                }
                
                return Expression.Lambda<Func<object, object, IResolveFieldContext, object>>(
                    Expression.Convert(call, typeof(object)),
                    p1, p2, p3).Compile();
            });

            return handleMethodInfo(commandHandler, command, context);
        }
    }
}
