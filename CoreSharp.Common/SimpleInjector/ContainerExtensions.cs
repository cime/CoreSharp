using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Extensions;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void RegisterPackages(this Container container)
        {
            foreach (var packageType in typeof(IPackage).Assembly.GetDependentAssemblies().SelectMany(x => x.DefinedTypes).Where(x => typeof(IPackage).IsAssignableFrom(x)))
            {
                var package = (IPackage) Activator.CreateInstance(packageType);
                package.Register(container);
            }
        }

        public static void RegisterPackage<T>(this Container container)
            where T : IPackage
        {
            var package = Activator.CreateInstance<T>();
            package.Register(container);
        }

        public static IEnumerable<object> TryGetAllInstances(this Container container, Type serviceType)
        {
            var collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            IServiceProvider provider = container;

            return (IEnumerable<object>)(provider.GetService(collectionType) ??
                     Activator.CreateInstance(typeof(List<>).MakeGenericType(serviceType)));
        }

        public static object TryGetInstance(this Container container, Type serviceType)
        {
            IServiceProvider provider = container;

            return provider.GetService(serviceType);
        }

        public static TInst GetRegisteredInstance<TInst>(this Container container)
            where TInst : class
        {

            var registration = container.GetCurrentRegistrations().FirstOrDefault(x => x.ServiceType == typeof(TInst));
            if (registration == null)
            {
                return null;
            }
            var creatorField = registration.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == "instanceCreator");
            if (creatorField == null)
            {
                return null;
            }
            var fnc = creatorField.GetValue(registration) as Func<object>;
            if (fnc == null)
            {
                return null;
            }
            var cfg = fnc() as TInst;
            return cfg;

        }
    }
}
