using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Command;
using CoreSharp.Common.Events;
using CoreSharp.Common.Exceptions;
using CoreSharp.Common.Extensions;
using CoreSharp.Common.Query;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void RegisterCommandHandlers(this Container container)
        {
            foreach (var dependentAssembly in typeof(ICommandHandler<>).Assembly.GetDependentAssemblies())
            {
                RegisterCommandHandlersFromAssembly(container, dependentAssembly);
            }
        }

        public static void RegisterCommandHandlersFromAssemblyOf<T>(this Container container)
        {
            RegisterCommandHandlersFromAssembly(container, typeof(T).GetTypeInfo().Assembly);
        }

        public static void RegisterCommandHandlersFromAssembly(this Container container, Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType &&
            (
                x.IsAssignableToGenericType(typeof(ICommandHandler<>)) ||
                x.IsAssignableToGenericType(typeof(ICommandHandler<,>)) ||
                x.IsAssignableToGenericType(typeof(IAsyncCommandHandler<>)) ||
                x.IsAssignableToGenericType(typeof(IAsyncCommandHandler<,>))
            ));

            foreach (var o in types.Select(t => new { Implementation = t, Services = t.ImplementedInterfaces }))
            {
                var type = o.Implementation.AsType();
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>() ?? new LifetimeAttribute(Lifetime.Transient);

                Registration registration;

                switch (attr.Lifetime)
                {
                    case Lifetime.Singleton:
                        registration = Lifestyle.Singleton.CreateRegistration(type, container);
                        break;
                    case Lifetime.Scoped:
                        registration = Lifestyle.Scoped.CreateRegistration(type, container);
                        break;
                    case Lifetime.Transient:
                        registration = Lifestyle.Transient.CreateRegistration(type, container);
                        break;
                    default:
                        throw new CoreSharpException($"Invalid {nameof(Lifetime)} value: {attr.Lifetime}");
                }

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.AddRegistration(serviceType, registration);
                }
            }
        }

        public static void RegisterQueryHandlers(this Container container)
        {
            foreach (var dependentAssembly in typeof(IQueryHandler<,>).Assembly.GetDependentAssemblies())
            {
                RegisterQueryHandlersFromAssembly(container, dependentAssembly);
            }
        }

        public static void RegisterQueryHandlersFromAssemblyOf<T>(this Container container)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;

            RegisterQueryHandlersFromAssembly(container, assembly);
        }

        public static void RegisterQueryHandlersFromAssembly(this Container container, Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType &&
                                                         (
                                                             x.IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                                                             x.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>))
                                                         ));

            foreach (var o in types.Select(t => new { Implementation = t, Services = t.ImplementedInterfaces }))
            {
                var type = o.Implementation.AsType();
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>() ?? new LifetimeAttribute(Lifetime.Transient);

                Registration registration;

                switch (attr.Lifetime)
                {
                    case Lifetime.Singleton:
                        registration = Lifestyle.Singleton.CreateRegistration(type, container);
                        break;
                    case Lifetime.Scoped:
                        registration = Lifestyle.Scoped.CreateRegistration(type, container);
                        break;
                    case Lifetime.Transient:
                        registration = Lifestyle.Transient.CreateRegistration(type, container);
                        break;
                    default:
                        throw new CoreSharpException($"Invalid {nameof(Lifetime)} value: {attr.Lifetime}");
                }

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.AddRegistration(serviceType, registration);
                }
            }
        }

        public static void RegisterEventHandlers(this Container container)
        {
            foreach (var dependentAssembly in typeof(IEventHandler<>).Assembly.GetDependentAssemblies())
            {
                RegisterCommandHandlersFromAssembly(container, dependentAssembly);
            }
        }

        public static void RegisterEventHandlersFromAssemblyOf<T>(this Container container)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;

            RegisterEventHandlersFromAssembly(container, assembly);
        }

        public static void RegisterEventHandlersFromAssembly(this Container container, Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType &&
                (
                    x.IsAssignableToGenericType(typeof(IEventHandler<>)) ||
                    x.IsAssignableToGenericType(typeof(IAsyncEventHandler<>))
                ));

            foreach (var o in types.Select(t => new { Implementation = t, Services = t.ImplementedInterfaces }))
            {
                var type = o.Implementation.AsType();
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>() ?? new LifetimeAttribute(Lifetime.Singleton);

                Registration registration;

                switch (attr.Lifetime)
                {
                    case Lifetime.Singleton:
                        registration = Lifestyle.Singleton.CreateRegistration(type, container);
                        break;
                    case Lifetime.Scoped:
                        registration = Lifestyle.Scoped.CreateRegistration(type, container);
                        break;
                    case Lifetime.Transient:
                        registration = Lifestyle.Transient.CreateRegistration(type, container);
                        break;
                    default:
                        throw new CoreSharpException($"Invalid {nameof(Lifetime)} value: {attr.Lifetime}");
                }

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.Collection.Append(serviceType, registration);
                }
            }
        }

        public static void RegisterCqrs(this Container container)
        {
            RegisterCommandHandlers(container);
            RegisterQueryHandlers(container);
            RegisterEventHandlers(container);
        }

        public static void RegisterCqrsFromAssemblyOf<T>(this Container container)
        {
            RegisterCommandHandlersFromAssemblyOf<T>(container);
            RegisterQueryHandlersFromAssemblyOf<T>(container);
            RegisterEventHandlersFromAssemblyOf<T>(container);
        }
    }
}
