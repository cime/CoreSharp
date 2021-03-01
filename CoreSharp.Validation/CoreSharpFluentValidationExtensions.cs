using System.Collections.Generic;
using CoreSharp.Common.Models;
using CoreSharp.Validation;
using CoreSharp.Validation.PropertyValidators;

// ReSharper disable once CheckNamespace
namespace FluentValidation
{
    public static class CoreSharpFluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PhoneNumberValidator());
        }

        public static IRuleBuilderOptions<T, string> Guid<T>(this IRuleBuilder<T, string> ruleBuilder, string format = "D")
        {
            return ruleBuilder.SetValidator(new GuidValidator(format));
        }

        public static void PagedListFilter<T>(this AbstractValidator<T> validator, PagingConfiguration configuration)
            where T : PagedListFilter
        {
            validator.RuleFor(x => x.Skip).GreaterThanOrEqualTo(0).LessThanOrEqualTo(configuration.MaxSkip);
            validator.RuleFor(x => x.Take).GreaterThan(0).LessThanOrEqualTo(configuration.MaxTake);
        }

        public static IRuleBuilderOptions<T, string> OneOf<T, TProp>(this IRuleBuilder<T, string> ruleBuilder, IEnumerable<TProp> validOptions, string errorMessage = "Value is not valid.")
        {
            return ruleBuilder.SetValidator(new OneOfValidator<TProp>(validOptions, errorMessage));
        }

    }
}
