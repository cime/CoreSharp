using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Validation;
using CoreSharp.Validation.Internal;
using FluentValidation;

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
            var matches = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && IsDomainValidator(t))
                .Select(t => new
                {
                    Implementation = t,
                    Services = t.GetInterfaces().Where(i => IsGenericDomainValidator(i))
                });

            Registration registration;
            var genericDomainValidatorTypes = new List<Type>();
            foreach (var match in matches)
            {
                if (match.Implementation.IsGenericType)
                {
                    var args = match.Implementation.GetGenericArguments();
                    if (args.Length > 2)
                    {
                        throw new NotSupportedException(
                                $"Domain validators with more that two generic arguments are not supported. Invalid domain validator: {match.Implementation}" +
                                "Hint: make the validator as an abstract type or modify the type to contain two generic arguments or less");
                    }

                    container.Register(match.Implementation, match.Implementation);
                    // Register the implementation as we will need the registration when adding root/child producers
                    // Open generic registrations cannot be retrieved with GetCurrentRegistrations so we have to store
                    // them separately
                    genericDomainValidatorTypes.Add(match.Implementation);
                    foreach (var serviceType in match.Services)
                    {
                        container.Collection.Append(serviceType, match.Implementation);
                    }

                    continue;
                }

                registration = Lifestyle.Transient.CreateRegistration(match.Implementation, container);
                container.AddRegistration(match.Implementation, registration);
                foreach (var serviceType in match.Services)
                {
                    container.Collection.Append(serviceType, registration);
                }
            }

            var metadata = new RegisteredValidationAssemblyMetadata(assembly, genericDomainValidatorTypes);
            container.Collection.AppendInstance(metadata);
        }

        private static bool IsGenericDomainValidator(Type type)
        {
            return
                type.IsAssignableToGenericType(typeof(IDomainValidator<,>)) ||
                type.IsAssignableToGenericType(typeof(IAsyncDomainValidator<,>));
        }

        private static bool IsDomainValidator(Type type)
        {
            return
                typeof(IDomainValidator).IsAssignableFrom(type) ||
                typeof(IAsyncDomainValidator).IsAssignableFrom(type);
        }
    }
}
