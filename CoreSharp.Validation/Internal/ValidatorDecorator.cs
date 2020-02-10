using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using SimpleInjector;

namespace CoreSharp.Validation.Internal
{
    internal class ValidatorDecorator<TModel> : IValidator<TModel>, IEnumerable<IValidationRule>
    {
        private static readonly Dictionary<InstanceProducer, IDomainValidator> EmptyDomainValidatorDictionary = new Dictionary<InstanceProducer, IDomainValidator>();
        private static readonly Dictionary<InstanceProducer, IAsyncDomainValidator> EmptyAsyncDomainValidatorDictionary = new Dictionary<InstanceProducer, IAsyncDomainValidator>();
        private static readonly string[] EmptyRuleSets = new string[0];
        private const string DomainValidatorsKey = "DomainValidators";
        private const string AsyncDomainValidatorsKey = "AsyncDomainValidators";

        private readonly AbstractValidator<TModel> _validator;
        private readonly ValidatorCache _cache;

        public ValidatorDecorator(IValidator<TModel> validator, ValidatorCache cache)
        {
            _validator = (AbstractValidator<TModel>)validator;
            _cache = cache;

            SetValidatorContextAsCustomState(_validator);
            cache.RegisterValidator(_validator);
            var addRuleMethod = _validator.GetType().GetMethod("AddRule", BindingFlags.Instance | BindingFlags.NonPublic);
            addRuleMethod.Invoke(_validator, new object[] { new DomainValidatorsValidator(GetRulesToValidate, GetAsyncRulesToValidate) });
        }

        private void SetValidatorContextAsCustomState(IEnumerable<IValidationRule> rules)
        {
            foreach (var propertyRule in rules.OfType<PropertyRule>().Where(o => o.CurrentValidator.Options.CustomStateProvider == null))
            {
                propertyRule.CurrentValidator.Options.CustomStateProvider = o => o;
            }
        }

        public ValidationResult Validate(object instance)
        {
            if (!CanValidateInstancesOfType(instance.GetType()))
            {
                throw new InvalidOperationException(
                    $"Cannot validate instances of type '{instance.GetType().Name}'. " +
                    $"This validator can only validate instances of type '{typeof(TModel).Name}'.");
            }

            return Validate((TModel) instance);
        }

        public Task<ValidationResult> ValidateAsync(object instance, CancellationToken cancellation = new CancellationToken())
        {
            if (!CanValidateInstancesOfType(instance.GetType()))
            {
                throw new InvalidOperationException(
                    $"Cannot validate instances of type '{instance.GetType().Name}'. " +
                    $"This validator can only validate instances of type '{typeof(TModel).Name}'.");
            }

            return ValidateAsync((TModel)instance, cancellation);
        }

        public ValidationResult Validate(ValidationContext context)
        {
            SetupContext(context);

            return ((IValidator)_validator).Validate(context);
        }

        public async Task<ValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = new CancellationToken())
        {
            await SetupContextAsync(context);

            return await ((IValidator)_validator).ValidateAsync(context, cancellation);
        }

        public ValidationResult Validate(TModel instance)
        {
            var context = new ValidationContext<TModel>(instance, new PropertyChain(), ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory());
            SetupContext(context);

            return _validator.Validate(context);
        }

        public async Task<ValidationResult> ValidateAsync(TModel instance, CancellationToken cancellation = new CancellationToken())
        {
            var context = new ValidationContext<TModel>(instance, new PropertyChain(), ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory());
            await SetupContextAsync(context);

            return await _validator.ValidateAsync(context, cancellation);
        }

        public IValidatorDescriptor CreateDescriptor()
        {
            return _validator.CreateDescriptor();
        }

        public bool CanValidateInstancesOfType(Type type)
        {
            return ((IValidator)_validator).CanValidateInstancesOfType(type);
        }

        public CascadeMode CascadeMode
        {
            get => _validator.CascadeMode;
            set => _validator.CascadeMode = value;
        }

        public IEnumerator<IValidationRule> GetEnumerator()
        {
            return _validator.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_validator).GetEnumerator();
        }

        private IEnumerable<IDomainValidator> GetRulesToValidate(ValidationContext context)
        {
            var dict = context.RootContextData.TryGetValue(DomainValidatorsKey, out var dictObj)
                ? dictObj as Dictionary<InstanceProducer, IDomainValidator> ?? EmptyDomainValidatorDictionary
                : EmptyDomainValidatorDictionary;

            foreach (var producer in _cache.GetChildProducers<TModel>().Where(o => dict.ContainsKey(o)))
            {
                yield return dict[producer];
            }
        }

        private IEnumerable<IAsyncDomainValidator> GetAsyncRulesToValidate(ValidationContext context)
        {
            var dict = context.RootContextData.TryGetValue(AsyncDomainValidatorsKey, out var dictObj)
                ? dictObj as Dictionary<InstanceProducer, IAsyncDomainValidator> ?? EmptyAsyncDomainValidatorDictionary
                : EmptyAsyncDomainValidatorDictionary;

            foreach (var producer in _cache.GetChildProducers<TModel>().Where(o => dict.ContainsKey(o)))
            {
                yield return dict[producer];
            }
        }

        private IEnumerable<KeyValuePair<InstanceProducer, T>> GetDomainValidatorInstances<T>()
        {
            foreach (var producer in _cache.GetRootProducers<TModel>())
            {
                if (producer.ServiceType.IsGenericType &&
                    producer.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // Skip when the validator is async but sync was requested
                    if (typeof(T).IsAssignableFrom(producer.ServiceType.GetGenericArguments()[0]))
                    {
                        continue;
                    }

                    var domainValidators = (IEnumerable<T>)producer.GetInstance();
                    foreach (var domainValidator in domainValidators)
                    {
                        yield return new KeyValuePair<InstanceProducer, T>(producer, domainValidator);
                    }
                }
                // Skip when the validator is async but sync was requested
                else if (typeof(T).IsAssignableFrom(producer.ServiceType))
                {
                    yield return new KeyValuePair<InstanceProducer, T>(producer, (T)producer.GetInstance());
                }
            }
        }

        private Dictionary<InstanceProducer, IDomainValidator> ExecuteBeforeValidation(object instance, ValidationContext context)
        {
            var list = new Dictionary<InstanceProducer, IDomainValidator>();
            foreach (var pair in GetDomainValidatorInstances<IDomainValidator>())
            {
                var domainValidator = pair.Value;
                if (CanExecuteBeforeValidation(domainValidator.RuleSets, context))
                {
                    list.Add(pair.Key, domainValidator); // TODO: fix duplicated keys
                    domainValidator.BeforeValidation(instance, context);
                }
            }

            return list;
        }

        private async Task<Dictionary<InstanceProducer, IAsyncDomainValidator>> ExecuteBeforeValidationAsync(object instance, ValidationContext context)
        {
            var list = new Dictionary<InstanceProducer, IAsyncDomainValidator>();
            foreach (var pair in GetDomainValidatorInstances<IAsyncDomainValidator>())
            {
                var domainValidator = pair.Value;
                if (CanExecuteBeforeValidation(domainValidator.RuleSets, context))
                {
                    list.Add(pair.Key, domainValidator); // TODO: fix duplicated keys
                    await domainValidator.BeforeValidationAsync(instance, context);
                }
            }

            return list;
        }

        private bool CanExecuteBeforeValidation(string[] ruleSets, ValidationContext context)
        {
            ruleSets = ruleSets ?? EmptyRuleSets;
            if (!ruleSets.Any() && !context.Selector.CanExecute(RuleSetValidationRule.GetRule(null), "", context))
            {
                return false;
            }

            if (ruleSets.Any() && !context.Selector.CanExecute(RuleSetValidationRule.GetRule(ruleSets), "", context))
            {
                return false;
            }

            return true;
        }

        private void SetupContext(ValidationContext context)
        {
            SetupContext(context, true);
        }

        private Task SetupContextAsync(ValidationContext context)
        {
            return SetupContextAsync(context, true);
        }

        private void SetupContext(ValidationContext context, bool setupAsync)
        {
            if (context.RootContextData.TryGetValue(DomainValidatorsKey, out var dictObj) && 
                dictObj is Dictionary<InstanceProducer, IDomainValidator> domainValidators)
            {
                var dict = ExecuteBeforeValidation(context.InstanceToValidate, context);
                foreach (var pair in dict)
                {
                    domainValidators[pair.Key] = pair.Value;
                }
            }
            else
            {
                context.RootContextData[DomainValidatorsKey] = ExecuteBeforeValidation(context.InstanceToValidate, context);
            }

            if (setupAsync)
            {
                SetupContextAsync(context, false).GetAwaiter().GetResult();
            }
        }

        private async Task SetupContextAsync(ValidationContext context, bool setupSync)
        {
            if (context.RootContextData.TryGetValue(AsyncDomainValidatorsKey, out var dictObj) &&
                dictObj is Dictionary<InstanceProducer, IAsyncDomainValidator> domainValidators)
            {
                var dict = await ExecuteBeforeValidationAsync(context.InstanceToValidate, context);
                foreach (var pair in dict)
                {
                    domainValidators[pair.Key] = pair.Value;
                }
            }
            else
            {
                context.RootContextData[AsyncDomainValidatorsKey] = await ExecuteBeforeValidationAsync(context.InstanceToValidate, context);
            }

            if (setupSync)
            {
                SetupContext(context, false);
            }
        }
    }
}
