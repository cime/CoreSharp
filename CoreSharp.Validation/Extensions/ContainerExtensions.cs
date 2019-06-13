using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Validation;
using FluentValidation;
using SimpleInjector.Advanced;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
    public static class ContainerExtensions
    {
        public static void RegisterValidatorsFromAssemblyOf<T>(this Container container)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;
            RegisterFluentValidators(container, assembly);
            RegisterDomainValidators(container, assembly);

            container.RegisterConditional(typeof(IValidator<>), typeof(Validator<>), Lifestyle.Singleton, o => !o.Handled);
        }

        private static void RegisterFluentValidators(Container container, Assembly assembly)
        {
            var matches = AssemblyScanner.FindValidatorsInAssembly(assembly)
                            .Where(match => !match.ValidatorType.GetTypeInfo().IsInterface &&
                                            !match.ValidatorType.GetTypeInfo().IsAbstract);

            foreach (var match in matches)
            {
                var serviceType = match.ValidatorType.GetGenericType(typeof(IValidator<>));
                if (serviceType == null)
                {
                    continue;
                }
                var registration = Lifestyle.Singleton.CreateRegistration(match.ValidatorType, container);
                container.AddRegistration(match.ValidatorType, registration);
                container.AddRegistration(serviceType, registration);
            }
        }

        private static void RegisterDomainValidators(Container container, Assembly assembly)
        {
            var matches = assembly.GetExportedTypes().Where(x =>
            {
                var typeInfo = x.GetTypeInfo();

                return typeInfo.IsClass && !typeInfo.IsAbstract && typeInfo.IsAssignableToGenericType(typeof(IDomainValidator<>));
            });

            foreach (var match in matches)
            {
                var serviceType = match.GetGenericType(typeof(IDomainValidator<>));
                if (serviceType == null)
                {
                    continue;
                }

                var registration = Lifestyle.Scoped.CreateRegistration(match, container);
                container.AddRegistration(match, registration);
                container.Collection.Append(serviceType, registration);
            }
        }
    }
}
