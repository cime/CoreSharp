using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace CoreSharp.Validation.Internal
{
internal class DomainValidatorsValidator : IValidationRule
    {
        private readonly Func<ValidationContext, IEnumerable<IDomainValidator>> _rulesFunc;
        private readonly Func<ValidationContext, IEnumerable<IAsyncDomainValidator>> _asyncRulesFunc;

        public IEnumerable<IPropertyValidator> Validators { get { yield break; } }
        public string[] RuleSets { get; set; }

        public DomainValidatorsValidator(
            Func<ValidationContext, IEnumerable<IDomainValidator>> rulesFunc,
            Func<ValidationContext, IEnumerable<IAsyncDomainValidator>> asyncRulesFunc
            )
        {
            _rulesFunc = rulesFunc;
            _asyncRulesFunc = asyncRulesFunc;
        }

        public IEnumerable<ValidationFailure> Validate(ValidationContext context)
        {
            return ValidateAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<ValidationFailure>> ValidateAsync(ValidationContext context, CancellationToken cancellation)
        {
            var list = new List<ValidationFailure>();
            foreach (var rule in _asyncRulesFunc(context))
            {
                if (!await rule.CanValidateAsync(context.InstanceToValidate, context))
                {
                    continue;
                }

                var result = await rule.ValidateAsync(context.InstanceToValidate, context);
                if (result == null)
                {
                    continue;
                }

                list.Add(result);
            }

            foreach (var rule in _rulesFunc(context).Where(o => o.CanValidate(context.InstanceToValidate, context)))
            {
                var result = rule.Validate(context.InstanceToValidate, context);
                if (result == null)
                {
                    continue;
                }

                list.Add(result);
            }

            return list;
        }

        public void ApplyCondition(Func<PropertyValidatorContext, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }

        public void ApplyAsyncCondition(Func<PropertyValidatorContext, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }

        public void ApplyCondition(Func<ValidationContext, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }

        public void ApplyAsyncCondition(Func<ValidationContext, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }

        public void ApplyCondition(Func<object, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }

        public void ApplyAsyncCondition(Func<object, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }
    }
}
