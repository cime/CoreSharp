﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CoreSharp.Validation.Extensions;
using FluentValidation.Validators;

namespace CoreSharp.Breeze
{
    internal static class FluentValidators
    {
        private static readonly Dictionary<Type, string> ValNames = new Dictionary<Type, string>();

        static FluentValidators()
        {
            ValNames.Add(typeof(NotNullValidator), "fvNotNull");
            ValNames.Add(typeof(EmailValidator), "fvEmail");
            ValNames.Add(typeof(NotEmptyValidator), "fvNotEmpty");
            ValNames.Add(typeof(EqualValidator), "fvEqual");
            ValNames.Add(typeof(ExclusiveBetweenValidator), "fvExclusiveBetween");
            ValNames.Add(typeof(InclusiveBetweenValidator), "fvInclusiveBetween");
            ValNames.Add(typeof(LengthValidator), "fvLength");
            ValNames.Add(typeof(NotEqualValidator), "fvNotEqual");
            ValNames.Add(typeof(RegularExpressionValidator), "fvRegularExpression");
            ValNames.Add(typeof(CreditCardValidator), "fvCreditCard");
            ValNames.Add(typeof(ExactLengthValidator), "fvExactLength");
            ValNames.Add(typeof(GreaterThanValidator), "fvGreaterThan");
            ValNames.Add(typeof(GreaterThanOrEqualValidator), "fvGreaterThanOrEqual");
            ValNames.Add(typeof(LessThanOrEqualValidator), "fvLessThanOrEqual");
            ValNames.Add(typeof(LessThanValidator), "fvLessThan");
            ValNames.Add(typeof(ScalePrecisionValidator), "fvScalePrecision");
        }

        public static Dictionary<Type, string> GetAllValidators()
        {
            return ValNames.ToDictionary(o => o.Key, o => o.Value);
        }

        public static string GetName(IPropertyValidator validator)
        {
            return GetName(validator.GetType());
        }

        private static string ToClientFormat(string format)
        {
            return Regex.Replace(format, @"{(.)(\w*)}", match => //TODO: Config
                    "{{" + match.Groups[1].Value.ToLowerInvariant() + match.Groups[2].Value + "}}",
                RegexOptions.IgnoreCase);
        }

        public static IDictionary<string, object> GetParameters(IPropertyValidator validator)
        {
            var result = new Dictionary<string, object>();
            result["errorMessageId"] = ToClientFormat(validator.GetMessageId());

            if (validator is IComparisonValidator equalVal)
            {
                //result["comparison"] = equalVal.Comparison.ToString();
                result["valueToCompare"] = equalVal.ValueToCompare;
                if (equalVal.MemberToCompare != null)
                    result["memberToCompare"] = equalVal.MemberToCompare.Name;
            }

            if (validator is IBetweenValidator btwVal)
            {
                result["from"] = btwVal.From;
                result["to"] = btwVal.To;
            }

            if (validator is ILengthValidator lengthVal)
            {
                result["min"] = lengthVal.Min;
                result["max"] = lengthVal.Max;
            }

            if (validator is IRegularExpressionValidator regexVal)
            {
                result["expression"] = regexVal.Expression;
            }

            if (validator is ScalePrecisionValidator scalePrecisionVal)
            {
                result["scale"] = scalePrecisionVal.Scale;
                result["precision"] = scalePrecisionVal.Precision;
            }

            return result;
        }

        public static string GetName(Type validatorType)
        {
            return ValNames.ContainsKey(validatorType)
                ? ValNames[validatorType]
                : null;
        }
    }
}
