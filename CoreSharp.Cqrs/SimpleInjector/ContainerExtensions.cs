using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Exceptions;
using CoreSharp.Common.Extensions;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Events;
using CoreSharp.Cqrs.Query;

#nullable disable

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Registers all command handlers from assemblies that depends on assembly of ICommandHandler&lt;&gt;
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public static void RegisterCommandHandlers(this Container container)
        {
            RegisterCommandHandlers(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterCommandHandlers(this Container container, Lifestyle lifestyle)
        {
            foreach (var dependentAssembly in typeof(ICommandHandler<>).Assembly.GetDependentAssemblies())
            {
                RegisterCommandHandlersFromAssembly(container, dependentAssembly, lifestyle);
            }
        }

        public static void RegisterCommandHandlersFromAssemblyOf<T>(this Container container)
        {
            RegisterCommandHandlersFromAssemblyOf<T>(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterCommandHandlersFromAssemblyOf<T>(this Container container, Lifestyle lifestyle)
        {
            RegisterCommandHandlersFromAssembly(container, typeof(T).GetTypeInfo().Assembly, lifestyle);
        }

        public static void RegisterCommandHandlersFromAssembly(this Container container, Assembly assembly)
        {
            RegisterCommandHandlersFromAssembly(container, assembly, container.Options.DefaultLifestyle);
        }

        public static void RegisterCommandHandlersFromAssembly(this Container container, Assembly assembly, Lifestyle lifestyle)
        {
            if (lifestyle == null)
            {
                throw new ArgumentNullException(nameof(lifestyle));
            }

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
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>();

                var registration = (attr?.Lifestyle ?? lifestyle).CreateRegistration(type, container);

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.AddRegistration(serviceType, registration);
                }
            }
        }

        /// <summary>
        /// Registers all command handlers from assemblies that depends on assembly of IQueryHandler&lt;&gt;
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public static void RegisterQueryHandlers(this Container container)
        {
            RegisterQueryHandlers(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterQueryHandlers(this Container container, Lifestyle lifestyle)
        {
            foreach (var dependentAssembly in typeof(IQueryHandler<,>).Assembly.GetDependentAssemblies())
            {
                RegisterQueryHandlersFromAssembly(container, dependentAssembly, lifestyle);
            }
        }

        public static void RegisterQueryHandlersFromAssemblyOf<T>(this Container container)
        {
            RegisterQueryHandlersFromAssemblyOf<T>(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterQueryHandlersFromAssemblyOf<T>(this Container container, Lifestyle lifestyle)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;

            RegisterQueryHandlersFromAssembly(container, assembly, lifestyle);
        }

        public static void RegisterQueryHandlersFromAssembly(this Container container, Assembly assembly, Lifestyle lifestyle)
        {
            if (lifestyle == null)
            {
                throw new ArgumentNullException(nameof(lifestyle));
            }

            var types = assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType &&
                                                         (
                                                             x.IsAssignableToGenericType(typeof(IQueryHandler<,>)) ||
                                                             x.IsAssignableToGenericType(typeof(IAsyncQueryHandler<,>))
                                                         ));

            foreach (var o in types.Select(t => new { Implementation = t, Services = t.ImplementedInterfaces }))
            {
                var type = o.Implementation.AsType();
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>();

                var registration = (attr?.Lifestyle ?? lifestyle).CreateRegistration(type, container);

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.AddRegistration(serviceType, registration);
                }
            }
        }

        /// <summary>
        /// Registers all event handlers from assemblies that depends on assembly of IEventHandler&lt;&gt;
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public static void RegisterEventHandlers(this Container container)
        {
            RegisterEventHandlers(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterEventHandlers(this Container container, Lifestyle lifestyle)
        {
            foreach (var dependentAssembly in typeof(IEventHandler<>).Assembly.GetDependentAssemblies())
            {
                RegisterCommandHandlersFromAssembly(container, dependentAssembly, lifestyle);
            }
        }

        public static void RegisterEventHandlersFromAssemblyOf<T>(this Container container)
        {
            RegisterEventHandlersFromAssemblyOf<T>(container, container.Options.DefaultLifestyle);
        }

        public static void RegisterEventHandlersFromAssemblyOf<T>(this Container container, Lifestyle lifestyle)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;

            RegisterEventHandlersFromAssembly(container, assembly, lifestyle);
        }

        public static void RegisterEventHandlersFromAssembly(this Container container, Assembly assembly, Lifestyle lifestyle)
        {
            if (lifestyle == null)
            {
                throw new ArgumentNullException(nameof(lifestyle));
            }

            var types = assembly.DefinedTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType &&
                (
                    x.IsAssignableToGenericType(typeof(IEventHandler<>)) ||
                    x.IsAssignableToGenericType(typeof(IAsyncEventHandler<>))
                ));

            foreach (var o in types.Select(t => new { Implementation = t, Services = t.ImplementedInterfaces }))
            {
                var type = o.Implementation.AsType();
                var attr = o.Implementation.GetCustomAttribute<LifetimeAttribute>();

                var registration = (attr?.Lifestyle ?? lifestyle).CreateRegistration(type, container);

                container.AddRegistration(type, registration);

                foreach (var serviceType in o.Services)
                {
                    container.Collection.Append(serviceType, registration);
                }
            }
        }

        /// <summary>
        /// Registers all command, query and event handlers
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public static void RegisterCqrs(this Container container)
        {
            RegisterCommandHandlers(container);
            RegisterQueryHandlers(container);
            RegisterEventHandlers(container);
        }

        public static void RegisterCqrs(this Container container, Lifestyle lifestyle)
        {
            RegisterCommandHandlers(container, lifestyle);
            RegisterQueryHandlers(container, lifestyle);
            RegisterEventHandlers(container, lifestyle);
        }

        /// <summary>
        /// Registers all command, query and event handlers from assembly of <typeparamref name="T" />
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public static void RegisterCqrsFromAssemblyOf<T>(this Container container)
        {
            RegisterCommandHandlersFromAssemblyOf<T>(container);
            RegisterQueryHandlersFromAssemblyOf<T>(container);
            RegisterEventHandlersFromAssemblyOf<T>(container);
        }

        public static void RegisterCqrsFromAssemblyOf<T>(this Container container, Lifestyle lifestyle)
        {
            RegisterCommandHandlersFromAssemblyOf<T>(container, lifestyle);
            RegisterQueryHandlersFromAssemblyOf<T>(container, lifestyle);
            RegisterEventHandlersFromAssemblyOf<T>(container, lifestyle);
        }
    }
}
