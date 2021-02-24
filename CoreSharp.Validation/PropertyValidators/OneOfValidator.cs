using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace CoreSharp.Validation
{

    public class OneOfValidator<T> : AbstractCommonPropertyValidator
    {
        private readonly IList<T> _validOptions;
        private readonly string _errorMessage;

        public OneOfValidator(IEnumerable<T> validoptions, string errorMessage = "Value is not valid.") : base(new LanguageStringSource(nameof(OneOfValidator<T>)))
        {
            _validOptions = validoptions.ToList();
            _errorMessage = errorMessage;
        }

        protected override string ErrorTemplate => _errorMessage;

        protected override bool IsValid(PropertyValidatorContext context)
        {
            try
            {
                var val = (T)context.PropertyValue;
                var valid = _validOptions.Contains(val);
                return valid;
            } catch(Exception)
            {
                return false;
            }

        }
    }
}
