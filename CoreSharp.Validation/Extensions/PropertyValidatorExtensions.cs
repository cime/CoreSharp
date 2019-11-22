using System;
using CoreSharp.Common.Internationalization;
using FluentValidation.Validators;

namespace CoreSharp.Validation.Extensions
{
    public static class PropertyValidatorExtensions
    {
        public static string GetMessageId(this IPropertyValidator propVal, bool includePropName = false)
        {
            switch (propVal)
            {
                case EmailValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' is not a valid email address.")
                        : I18N.Register("Is not a valid email address.");
                case GreaterThanOrEqualValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be greater than or equal to '{ComparisonValue}'.")
                        : I18N.Register("Must be greater than or equal to '{ComparisonValue}'.");
                case GreaterThanValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be greater than '{ComparisonValue}'.")
                        : I18N.Register("Must be greater than '{ComparisonValue}'.");
                case MinimumLengthValidator _:
                    return includePropName
                        ? I18N.Register("The length of '{PropertyName}' must be at least {MinLength} characters. You entered {TotalLength} characters.")
                        : I18N.Register("The length must be at least {MinLength} characters. You entered {TotalLength} characters.");
                case MaximumLengthValidator _:
                    return includePropName
                        ? I18N.Register("The length of '{PropertyName}' must be {MaxLength} characters or fewer. You entered {TotalLength} characters.")
                        : I18N.Register("The length must be {MaxLength} characters or fewer. You entered {TotalLength} characters.");
                case ExactLengthValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be {MaxLength} characters in length. You entered {TotalLength} characters.")
                        : I18N.Register("Must be {MaxLength} characters in length. You entered {TotalLength} characters.");
                case LengthValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.")
                        : I18N.Register("Must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.");
                case LessThanOrEqualValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be less than or equal to '{ComparisonValue}'.")
                        : I18N.Register("Must be less than or equal to '{ComparisonValue}'.");
                case LessThanValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be less than '{ComparisonValue}'.")
                        : I18N.Register("Must be less than '{ComparisonValue}'.");
                case NotNullValidator _:
                case NotEmptyValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must not be empty.")
                        : I18N.Register("Must not be empty.");
                case NotEqualValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must not be equal to '{ComparisonValue}'.")
                        : I18N.Register("Must not be equal to '{ComparisonValue}'.");
                case AsyncPredicateValidator _:
                case PredicateValidator _:
                    return includePropName
                        ? I18N.Register("The specified condition was not met for '{PropertyName}'.")
                        : I18N.Register("The specified condition was not met.");
                case RegularExpressionValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' is not in the correct format.")
                        : I18N.Register("Is not in the correct format.");
                case EqualValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be equal to '{ComparisonValue}'.")
                        : I18N.Register("Must be equal to '{ComparisonValue}'.");
                case InclusiveBetweenValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be between {From} and {To}. You entered {Value}.")
                        : I18N.Register("Must be between {From} and {To}. You entered {Value}.");
                case ExclusiveBetweenValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be between {From} and {To} (exclusive). You entered {Value}.")
                        : I18N.Register("Must be between {From} and {To} (exclusive). You entered {Value}.");
                case CreditCardValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' is not a valid credit card number.")
                        : I18N.Register("Is not a valid credit card number.");
                case ScalePrecisionValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.")
                        : I18N.Register("Must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.");
                case EmptyValidator _:
                case NullValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' must be empty.")
                        : I18N.Register("Must be empty.");
                case EnumValidator _:
                    return includePropName
                        ? I18N.Register("'{PropertyName}' has a range of values which does not include '{PropertyValue}'.")
                        : I18N.Register("'Has a range of values which does not include '{PropertyValue}'.");
                default:
                    throw new NotSupportedException($"Validator of type '{propVal}' has not a predefined localized message. Pass the localized message as parameter");
            }
        }
    }
}
