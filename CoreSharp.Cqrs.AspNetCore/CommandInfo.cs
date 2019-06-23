using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Command;

namespace CoreSharp.Cqrs.AspNetCore
{
    [DebuggerDisplay("{CommandType.Name,nq}")]
    public sealed class CommandInfo
    {
        public readonly Type CommandType;
        public readonly Type CommandHandlerType;
        public readonly Type ResultType;
        public readonly bool IsAsync;
        public readonly bool IsGeneric;
        public Type[] GenericTypes;

        public CommandInfo(Type commandType)
        {
            CommandType = commandType;
            IsAsync = typeof(IAsyncCommand).IsAssignableFrom(commandType) ||
                      commandType.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncCommand<>));
            IsGeneric = commandType.GetTypeInfo().IsAssignableToGenericType(typeof(ICommand<>)) ||
                        commandType.GetTypeInfo().IsAssignableToGenericType(typeof(IAsyncCommand<>));

            GenericTypes = new[] { CommandType };

            if (IsGeneric)
            {
                ResultType = DetermineResultTypes(commandType, IsAsync).Single();
                GenericTypes = GenericTypes.Union(new[] { ResultType }).ToArray();
            }

            if (IsAsync)
            {
                if (IsGeneric)
                {
                    CommandHandlerType = typeof(IAsyncCommandHandler<,>).MakeGenericType(GenericTypes);
                }
                else
                {
                    CommandHandlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(GenericTypes);
                }
            }
            else
            {
                if (IsGeneric)
                {
                    CommandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(GenericTypes);
                }
                else
                {
                    CommandHandlerType = typeof(ICommandHandler<>).MakeGenericType(GenericTypes);
                }
            }
        }

        private static IEnumerable<Type> DetermineResultTypes(Type type, bool isAsync) =>
            from interfaceType in type.GetInterfaces()
            where interfaceType.GetTypeInfo().IsGenericType
            where (!isAsync && interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>)) ||
                  (isAsync && interfaceType.GetGenericTypeDefinition() == typeof(IAsyncCommand<>))
            select interfaceType.GetGenericArguments()[0];
    }
}
