using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Internal;
using FluentValidation.Results;

// ReSharper disable once CheckNamespace
namespace FluentValidation
{
    public static class ValidatorExtensions
    {
        public static void ValidateAndThrow<T>(this IValidator<T> validator, T instance, string[] ruleSets)
        {
            var valResult = Validate(validator, instance, ruleSets, null);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T instance, string[] ruleSets)
        {
            var valResult = await ValidateAsync(validator, instance, ruleSets, null);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        public static ValidationResult Validate<T>(this IValidator<T> validator, T instance, string[] ruleSets)
        {
            return Validate(validator, instance, ruleSets, null);
        }

        public static Task<ValidationResult> ValidateAsync<T>(this IValidator<T> validator, T instance, string[] ruleSets)
        {
            return ValidateAsync(validator, instance, ruleSets, null);
        }

        public static ValidationResult Validate(IValidator validator, object instance, string[] ruleSets,
            Dictionary<string, object> extraData = null)
        {
            IValidatorSelector selector = new DefaultValidatorSelector();
            if (ruleSets != null)
            {
                selector = ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory(ruleSets);
            }
            var context = new ValidationContext(instance, new PropertyChain(), selector);
            if (extraData != null)
            {
                foreach (var pair in extraData)
                {
                    context.RootContextData[pair.Key] = pair.Value;
                }
            }
            return validator.Validate(context);
        }

        public static Task<ValidationResult> ValidateAsync(IValidator validator, object instance, string[] ruleSets,
            Dictionary<string, object> extraData = null)
        {
            IValidatorSelector selector = new DefaultValidatorSelector();
            if (ruleSets != null)
            {
                selector = ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory(ruleSets);
            }
            var context = new ValidationContext(instance, new PropertyChain(), selector);
            if (extraData != null)
            {
                foreach (var pair in extraData)
                {
                    context.RootContextData[pair.Key] = pair.Value;
                }
            }
            return validator.ValidateAsync(context);
        }
    }
}
