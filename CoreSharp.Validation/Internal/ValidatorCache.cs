using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using SimpleInjector;

namespace CoreSharp.Validation.Internal
{
    internal class ValidatorCache
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<InstanceProducer, bool>> _childProducers =
            new ConcurrentDictionary<Type, ConcurrentDictionary<InstanceProducer, bool>>();

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<InstanceProducer, bool>> _rootProducers =
            new ConcurrentDictionary<Type, ConcurrentDictionary<InstanceProducer, bool>>();

        private readonly HashSet<Type> _genericDomainValidatorTypes = new HashSet<Type>();
        private readonly Container _container;

        public ValidatorCache(Container container, IEnumerable<RegisteredValidationAssemblyMetadata> registeredValidationAssemblies)
        {
            _container = container;
            foreach (var genericDomainValidatorType in registeredValidationAssemblies.SelectMany(o => o.GenericDomainValidatorTypes))
            {
                _genericDomainValidatorTypes.Add(genericDomainValidatorType);
            }
        }

        public void AddChildProducers<T>(IEnumerable<InstanceProducer> producers)
        {
            var set = _childProducers.GetOrAdd(typeof(T), new ConcurrentDictionary<InstanceProducer, bool>());
            producers.ForEach(o => set.GetOrAdd(o, true));
        }

        public void AddChildProducers(Type type, params InstanceProducer[] producers)
        {
            var set = _childProducers.GetOrAdd(type, new ConcurrentDictionary<InstanceProducer, bool>());
            producers.ForEach(o => set.GetOrAdd(o, true));
        }

        public IEnumerable<InstanceProducer> GetChildProducers<T>()
        {
            return _childProducers.GetOrAdd(typeof(T), new ConcurrentDictionary<InstanceProducer, bool>()).Keys;
        }

        public void AddRootProducers<T>(params InstanceProducer[] producers)
        {
            var set = _rootProducers.GetOrAdd(typeof(T), new ConcurrentDictionary<InstanceProducer, bool>());
            producers.ForEach(o => set.GetOrAdd(o, true));
        }

        public void AddRootProducers<T>(IEnumerable<InstanceProducer> producers)
        {
            var set = _rootProducers.GetOrAdd(typeof(T), new ConcurrentDictionary<InstanceProducer, bool>());
            producers.ForEach(o => set.GetOrAdd(o, true));
        }

        public IEnumerable<InstanceProducer> GetRootProducers<T>()
        {
            return _rootProducers.GetOrAdd(typeof(T), new ConcurrentDictionary<InstanceProducer, bool>()).Keys;
        }

        public void RegisterValidator<TModel>(AbstractValidator<TModel> validator)
        {
            var registrations = _container.GetCurrentRegistrations()
                .Where(o => o.ServiceType.IsInterface && o.ServiceType.IsGenericType && IsDomainValidator(o.ServiceType))
                .Select(o => new
                {
                    Producer = o,
                    Type = GetDomainValidatorGenericType(o.ServiceType)
                })
                .Where(o => o.Type != null)
                .ToList();
            AddRootProducers<TModel>(registrations
                .Where(o => o.Type.GetGenericArguments()[0] == typeof(TModel))
                .Select(o => o.Producer));
            AddChildProducers<TModel>(registrations
                .Where(o => o.Type.GetGenericArguments()[1] == typeof(TModel))
                .Select(o => o.Producer));

            AddProducersForGenericRules(validator);
        }

        private void AddProducersForGenericRules<TModel>(AbstractValidator<TModel> validator)
        {
            var childModels = new HashSet<Type>();
            var validationRules = new List<IValidationRule>(validator);
            // Find all child models that are related to this one by traversing all validation rules
            while (validationRules.Count > 0)
            {
                var validationRule = validationRules.First();
                validationRules.Remove(validationRule);
                foreach (var propVal in validationRule.Validators)
                {
                    Type childValidationType = null;
                    if (propVal is IChildValidatorAdaptor childRule)
                    {
                        childValidationType = childRule.ValidatorType.GetGenericType(typeof(IValidator<>));
                    }

                    if (childValidationType == null)
                    {
                        continue;
                    }

                    var childModel = childValidationType.GetGenericArguments()[0];
                    if (childModel == typeof(TModel))
                    {
                        continue;
                    }

                    childModels.Add(childModel);
                    validationRules.AddRange(_container.GetInstance(childValidationType) as IEnumerable<IValidationRule> ?? Enumerable.Empty<IValidationRule>());
                }
            }

            // Add root and child producers for the valid root/child combinations
            foreach (var genericDomainValidatorType in _genericDomainValidatorTypes)
            {
                var domainValidatorType = 
                    genericDomainValidatorType.GetGenericType(typeof(IDomainValidator<,>)) ??
                    genericDomainValidatorType.GetGenericType(typeof(IAsyncDomainValidator<,>));
                var rootType = domainValidatorType.GetGenericArguments()[0];
                var childType = domainValidatorType.GetGenericArguments()[1];

                // Check if the current model can be applied as TRoot
                if (rootType == typeof(TModel) ||
                    (rootType.IsGenericParameter && rootType.GetGenericParameterConstraints().All(o => o.IsAssignableFrom(typeof(TModel)))))
                {
                    if (rootType == childType)
                    {
                        var genArgs = genericDomainValidatorType.GetGenericArguments().Select(args => typeof(TModel)).ToArray();
                        var producer = _container.GetRegistration(genericDomainValidatorType.MakeGenericType(genArgs.ToArray()));
                        AddRootProducers<TModel>(producer);
                        AddChildProducers(typeof(TModel), producer);
                    }

                    foreach (var childModel in childModels)
                    {
                        // Check if the current childmodel can be applied as TChild
                        if (childType == childModel ||
                            (childType.IsGenericParameter &&
                             childType.GetGenericParameterConstraints().All(o => o.IsAssignableFrom(childModel))))
                        {
                            var genArgs = new List<Type>();
                            if (rootType.IsGenericParameter)
                            {
                                genArgs.Add(typeof(TModel));
                            }

                            if (childType.IsGenericParameter)
                            {
                                genArgs.Add(childModel);
                            }

                            // Get registration by implementation type in order to avoid duplicates 
                            var producer = _container.GetRegistration(genericDomainValidatorType.MakeGenericType(genArgs.ToArray()));
                            AddRootProducers<TModel>(producer);
                            AddChildProducers(childModel, producer);
                        }
                    }
                }
            }
        }

        private static bool IsDomainValidator(Type type)
        {
            var genericDefiniton = type.GetGenericTypeDefinition();
            return genericDefiniton == typeof(IDomainValidator<,>) || genericDefiniton == typeof(IAsyncDomainValidator<,>);
        }

        private static Type GetDomainValidatorGenericType(Type type)
        {
            return type.GetGenericType(typeof(IDomainValidator<,>)) ?? type.GetGenericType(typeof(IAsyncDomainValidator<,>));
        }
    }
}
