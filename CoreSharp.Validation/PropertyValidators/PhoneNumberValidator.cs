using System.Text.RegularExpressions;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace CoreSharp.Validation.PropertyValidators
{

    public class PhoneNumberValidator : AbstractCommonPropertyValidator
    {

        private static Regex _regex = new Regex(@"^[0-9]{2,20}$");

        protected override string ErrorTemplate => "Value is not a valid phone number.";

        public PhoneNumberValidator() : base(new LanguageStringSource(nameof(PhoneNumberValidator)))
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var phone = context.PropertyValue?.ToString();
            return _regex.IsMatch(phone);
        }
    }
}
