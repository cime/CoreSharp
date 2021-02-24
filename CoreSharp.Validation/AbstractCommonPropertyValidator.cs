using FluentValidation.Resources;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace CoreSharp.Validation
{

    public abstract class AbstractCommonPropertyValidator : PropertyValidator
    {
        protected AbstractCommonPropertyValidator(IStringSource errorMessageSource) : base(errorMessageSource)
        {
        }

        protected abstract string ErrorTemplate { get; }

        protected override ValidationFailure CreateValidationError(PropertyValidatorContext context)
        {
            var failure = base.CreateValidationError(context);
            failure.ErrorMessage = context.MessageFormatter.BuildMessage(ErrorTemplate);
            return failure;
        }
    }

}
