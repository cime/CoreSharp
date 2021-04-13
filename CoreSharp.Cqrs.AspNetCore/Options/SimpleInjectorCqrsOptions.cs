using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;
using SimpleInjector;

namespace CoreSharp.Cqrs.AspNetCore.Options
{
    public class SimpleInjectorCqrsOptions : AbstractCqrsOptions
    {
        private readonly Container _container;

        public SimpleInjectorCqrsOptions(Container container)
        {
            _container = container;
        }

        public override IEnumerable<QueryInfo> GetQueryTypes()
        {
            if (!_container.IsLocked)
            {
                throw new InvalidOperationException("Container is not Locked");
            }

            return _container.GetCurrentRegistrations()
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
                .Select(t => new QueryInfo(t, this));
        }

        public override IEnumerable<CommandInfo> GetCommandTypes()
        {
            if (!_container.IsLocked)
            {
                throw new InvalidOperationException("Container is not Locked");
            }

            return _container.GetCurrentRegistrations()
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
                .Select(t => new CommandInfo(t, this));
        }

        public override object GetInstance(Type type)
        {
            return _container.GetInstance(type);
        }
    }
}
