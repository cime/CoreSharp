using System;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace CoreSharp.Validation.PropertyValidators
{
    public class GuidValidator : AbstractCommonPropertyValidator
    {
        private readonly string _format;

        public GuidValidator(string format = "D") : base(new LanguageStringSource(nameof(GuidValidator)))
        {
            _format = format;
        }

        protected override string ErrorTemplate => "Value is not a valid GUID.";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            bool valid = Guid.TryParseExact(context.PropertyValue as string, _format, out Guid guid);
            return valid;
        }
    }
}
