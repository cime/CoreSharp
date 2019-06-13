using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using SimpleInjector;

namespace CoreSharp.Validation.Internal
{
    internal class ValidatorDecorator<TModel> : IValidator<TModel>, IEnumerable<IValidationRule>
    {
        private readonly AbstractValidator<TModel> _validator;
        private readonly Container _container;

        public ValidatorDecorator(IValidator<TModel> validator, Container container)
        {
            _validator = (AbstractValidator<TModel>)validator;
            _container = container;

            var abstractValidator = (AbstractValidator<TModel>)validator;
            var addRuleMethod = abstractValidator.GetType().GetMethod("AddRule", BindingFlags.Instance | BindingFlags.NonPublic);
            addRuleMethod.Invoke(abstractValidator, new object[] { new DomainValidatorsValidator(GetRulesToValidate) });
        }

        private IEnumerable<IDomainValidator> GetRulesToValidate(ValidationContext context)
        {
            var domainValidatorType = typeof(IDomainValidator<TModel>);

            try
            {
                var domainValidators = (IEnumerable<IDomainValidator<TModel>>) _container.GetAllInstances(domainValidatorType);

                domainValidators = domainValidators.Where(x =>
                {
                    var validRuleSets = x.RuleSets ?? new string[] { };

                    if (!validRuleSets.Any() &&
                        !context.Selector.CanExecute(RuleSetValidationRule.GetRule(null), "", context))
                    {
                        return false;
                    }

                    if (validRuleSets.Any() && !context.Selector.CanExecute(RuleSetValidationRule.GetRule(validRuleSets), "", context))
                    {
                        return false;
                    }

                    return true;
                });

                return domainValidators;
            }
            catch (ActivationException)
            {
                return new List<IDomainValidator>();
            }
        }

        public ValidationResult Validate(object instance)
        {
            return Validate((TModel) instance);
        }

        public Task<ValidationResult> ValidateAsync(object instance, CancellationToken cancellation = new CancellationToken())
        {
            return ValidateAsync((TModel)instance, cancellation);
        }

        public ValidationResult Validate(ValidationContext context)
        {
            return ((IValidator)_validator).Validate(context);
        }

        public Task<ValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = new CancellationToken())
        {
            return ((IValidator)_validator).ValidateAsync(context, cancellation);
        }

        public ValidationResult Validate(TModel instance)
        {
            return _validator.Validate(instance);
        }

        public Task<ValidationResult> ValidateAsync(TModel instance, CancellationToken cancellation = new CancellationToken())
        {
            return _validator.ValidateAsync(instance, cancellation);
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
    }
}
