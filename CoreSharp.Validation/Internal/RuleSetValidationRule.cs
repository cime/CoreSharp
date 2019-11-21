using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace CoreSharp.Validation.Internal
{
    internal class RuleSetValidationRule : IValidationRule
    {
        private static readonly ConcurrentDictionary<string[], RuleSetValidationRule> Instances =
            new ConcurrentDictionary<string[], RuleSetValidationRule>();

        private static readonly RuleSetValidationRule DefaultRuleSetValidationRule = new RuleSetValidationRule();

        public static IValidationRule GetRule(string[] ruleSets)
        {
            if (ruleSets == null)
            {
                return DefaultRuleSetValidationRule;
            }

            return Instances.GetOrAdd(ruleSets, o => new RuleSetValidationRule(o));
        }

        static RuleSetValidationRule()
        {
        }

        public RuleSetValidationRule()
        {
            RuleSets = new string [0];
        }

        private RuleSetValidationRule(string[] ruleSets)
        {
            RuleSets = ruleSets;
        }

        IEnumerable<ValidationFailure> IValidationRule.Validate(ValidationContext context)
        {
            throw new NotSupportedException();
        }

        Task<IEnumerable<ValidationFailure>> IValidationRule.ValidateAsync(ValidationContext context, CancellationToken cancellation)
        {
            throw new NotSupportedException();
        }

        IEnumerable<IPropertyValidator> IValidationRule.Validators { get { yield break; } }
        public string[] RuleSets { get; set; }

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

        void ApplyCondition(Func<object, bool> predicate, ApplyConditionTo applyConditionTo)
        {
            throw new NotSupportedException();
        }

        public void ApplyAsyncCondition(Func<object, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
        {
            throw new NotImplementedException();
        }
    }
}
