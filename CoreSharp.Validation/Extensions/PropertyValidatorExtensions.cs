using System;
using CoreSharp.Common.Internationalization;
using FluentValidation.Validators;

namespace CoreSharp.Validation.Extensions
{
    public static class PropertyValidatorExtensions
    {
        public static string GetMessageId(this IPropertyValidator propVal, bool includePropName = false)
        {
            var propValType = propVal.GetType();

            #region NotNullValidator
            if (propValType == typeof(NotNullValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' is required.")
                    : I18N.Register("Is required.");
            }
            #endregion
            #region EmailValidator
            if (propValType == typeof(EmailValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' is not a valid email address.")
                    : I18N.Register("Is not a valid email address.");
            }
            #endregion
            #region NotEmptyValidator
            if (propValType == typeof(NotEmptyValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' should not be empty.")
                    : I18N.Register("Should not be empty.");
            }
            #endregion
            #region EqualValidator
            if (propValType == typeof(EqualValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' should be equal to '{ComparisonValue}'.")
                    : I18N.Register("Should be equal to '{ComparisonValue}'."));
            }
            #endregion
            #region ExclusiveBetweenValidator
            if (propValType == typeof(ExclusiveBetweenValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' must be between {From} and {To} (exclusive).")
                    : I18N.Register("Must be between {From} and {To} (exclusive).");
            }
            #endregion
            #region InclusiveBetweenValidator
            if (propValType == typeof(InclusiveBetweenValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' must be between {From} and {To}.")
                    : I18N.Register("Must be between {From} and {To}.");
            }
            #endregion
            #region LengthValidator
            if (propValType == typeof(LengthValidator))
            {
                var val = (LengthValidator)propVal;
                return (val.Min > 0)
                    ? (includePropName
                        ? I18N.Register("'{PropertyName}' must be between {Min} and {Max} characters.")
                        : I18N.Register("Must be between {Min} and {Max} characters."))
                    : (includePropName
                        ? I18N.Register("'{PropertyName}' must be less than or equal to {Max} characters.")
                        : I18N.Register("Must be less than or equal to {Max} characters."));
            }
            #endregion
            #region NotEqualValidator
            if (propValType == typeof(NotEqualValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' should not be equal to '{ComparisonValue}'.")
                    : I18N.Register("Should not be equal to '{ComparisonValue}'."));
            }
            #endregion
            #region RegularExpressionValidator
            if (propValType == typeof(RegularExpressionValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' is not in the correct format.")
                    : I18N.Register("Is not in the correct format.");
            }
            #endregion
            #region CreditCardValidator
            if (propValType == typeof(CreditCardValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' is not a valid credit card number.")
                    : I18N.Register("Is not a valid credit card number.");
            }
            #endregion
            #region ExactLengthValidator
            if (propValType == typeof(ExactLengthValidator))
            {
                return includePropName
                    ? I18N.Register("'{PropertyName}' must be {Max} characters in length.")
                    : I18N.Register("Must be {Max} characters in length.");
            }
            #endregion
            #region GreaterThanValidator
            if (propValType == typeof(GreaterThanValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' must be greater than '{ComparisonValue}'.")
                    : I18N.Register("Must be greater than '{ComparisonValue}'."));
            }
            #endregion
            #region GreaterThanOrEqualValidator
            if (propValType == typeof(GreaterThanOrEqualValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' must be greater than or equal to '{ComparisonValue}'.")
                    : I18N.Register("Must be greater than or equal to '{ComparisonValue}'."));
            }
            #endregion
            #region LessThanOrEqualValidator
            if (propValType == typeof(LessThanOrEqualValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' must be less than or equal to '{ComparisonValue}'.")
                    : I18N.Register("Must be less than or equal to '{ComparisonValue}'."));
            }
            #endregion
            #region LessThanValidator
            if (propValType == typeof(LessThanValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' must be less than '{ComparisonValue}'.")
                    : I18N.Register("Must be less than '{ComparisonValue}'."));
            }
            #endregion
            #region ScalePrecisionValidator
            if (propValType == typeof(ScalePrecisionValidator))
            {
                return (includePropName
                    ? I18N.Register("'{PropertyName}' must have scale '{Scale}', precision '{Precision}'.")
                    : I18N.Register("Must have scale '{Scale}', precision '{Precision}'."));
            }
            #endregion
            throw new NotSupportedException(
                string.Format(
                    "Validator of type '{0}' has not a predefined localized message. Pass the localized message as parameter",
                    propValType));
        }
    }
}
