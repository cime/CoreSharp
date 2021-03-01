using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.Validation.Abstractions;
using CoreSharp.Validation.Abstractions.Attributes;
using CoreSharp.Validation.Extensions;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace CoreSharp.Validation
{
    public class Validator<TModel> : AbstractValidator<TModel>
    {
        public Validator()
        {
            AddAttributeValidation();
        }

        protected ValidationFailure Failure(Expression<Func<TModel, object>> propertyExp, string errorMessage)
        {
            return new ValidationFailure(propertyExp.GetFullPropertyName(), errorMessage);
        }

        protected ValidationFailure Failure(string errorMessage)
        {
            return new ValidationFailure("", errorMessage);
        }

        protected ValidationFailure Success => null;

        protected void RuleSet(IEnumerable<string> ruleSetNames, Action action)
        {
            foreach (var ruleSet in ruleSetNames)
            {
                RuleSet(ruleSet, action);
            }
        }

        private void AddAttributeValidation()
        {
            var type = typeof(TModel);
            var ignoreAttrsAttr = type.GetTypeInfo().GetCustomAttributes(typeof(IgnoreValidationAttributesAttribute), true)
                .FirstOrDefault() as IgnoreValidationAttributesAttribute;
            var ignoreProps = ignoreAttrsAttr != null
                ? new HashSet<string>(ignoreAttrsAttr.Properties ?? new string[0])
                : new HashSet<string>();

            foreach (var prop in type.GetProperties().Where(o => !ignoreProps.Contains(o.Name)))
            {
                var attrs = prop.GetCustomAttributes(true);
                //Add validation from attributes
                foreach (var attr in attrs.OfType<ValidationAttribute>())
                {
                    var propValidator = CreatePropertyValidator(attr, type, prop);
                    if (propValidator == null)
                    {
                        continue;
                    }

                    AddAttributePropertyValidator(propValidator, prop, attr.IncludePropertyName);
                }
            }
        }

        private IPropertyValidator CreatePropertyValidator(ValidationAttribute validationAttribute, Type type, PropertyInfo prop)
        {
            switch (validationAttribute)
            {
                case NotNullAttribute _:
                    return prop.PropertyType == typeof(string)
                        ? (IPropertyValidator)new NotEmptyValidator(null)
                        : new NotNullValidator();
                case EmailAttribute _:
                    return new EmailValidator();
                case EnumAttribute enumAttr:
                    return new EnumValidator(enumAttr.Type);
                case EqualAttribute equalAttr:
                    return CreateComparisonValidator(equalAttr, type, prop,
                        o => new EqualValidator(o),
                        (func, info) => new EqualValidator(func, info));
                case NotEqualAttribute notEqualAttr:
                    return CreateComparisonValidator(notEqualAttr, type, prop,
                        o => new NotEqualValidator(o),
                        (func, info) => new NotEqualValidator(func, info));
                case LengthAttribute lengthAttr:
                    return lengthAttr.IsMinSet()
                        ? new LengthValidator(lengthAttr.Min, lengthAttr.Max)
                        : new MaximumLengthValidator(lengthAttr.Max);
                case RegularExpressionAttribute regexAttr:
                    return new RegularExpressionValidator(regexAttr.Expression, regexAttr.RegexOptions);
                case CreditCardAttribute _:
                    return new CreditCardValidator();
                case ScalePrecisionAttribute scalePrecisionAttr:
                    return new ScalePrecisionValidator(scalePrecisionAttr.Scale, scalePrecisionAttr.Precision);
                case ExactLengthAttribute exactLengthAttr:
                    return new ExactLengthValidator(exactLengthAttr.Length);
                case ExclusiveBetweenAttribute exclusiveBetweenAttribute:
                    return CreateBetweenValidator(exclusiveBetweenAttribute.From, exclusiveBetweenAttribute.To,
                        (from, to) => new ExclusiveBetweenValidator(from, to));
                case InclusiveBetweenAttribute inclusiveBetweenAttribute:
                    return CreateBetweenValidator(inclusiveBetweenAttribute.From, inclusiveBetweenAttribute.To,
                        (from, to) => new InclusiveBetweenValidator(from, to));
                case GreaterThanAttribute greaterThanAttr:
                    return CreateComparisonValidator(greaterThanAttr, type, prop,
                        o => new GreaterThanValidator(o as IComparable),
                        (func, info) => new GreaterThanValidator(func, info));
                case GreaterThanOrEqualAttribute greaterThanOrEqualsAttr:
                    return CreateComparisonValidator(greaterThanOrEqualsAttr, type, prop,
                        o => new GreaterThanOrEqualValidator(o as IComparable),
                        (func, info) => new GreaterThanOrEqualValidator(func, info));
                case LessThanOrEqualAttribute lessThanOrEqualsAttr:
                    return CreateComparisonValidator(lessThanOrEqualsAttr, type, prop,
                        o => new LessThanOrEqualValidator(o as IComparable),
                        (func, info) => new LessThanOrEqualValidator(func, info));
                case LessThanAttribute lessThanAttr:
                    return CreateComparisonValidator(lessThanAttr, type, prop,
                        o => new LessThanValidator(o as IComparable),
                        (func, info) => new LessThanValidator(func, info));
                default:
                    return null;
            }
        }

        private IPropertyValidator CreateBetweenValidator(object from, object to, Func<IComparable, IComparable, IPropertyValidator> createFunc)
        {
            if (from != null && to != null && from.GetType() != to.GetType())
            {
                var fromConverted = Convert.ChangeType(from, to.GetType());
                return createFunc(fromConverted as IComparable, to as IComparable);
            }
            else
            {
                return createFunc(from as IComparable, to as IComparable);
            }
        }

        private IComparisonValidator CreateComparisonValidator<T>(
            T attr,
            Type type,
            PropertyInfo prop,
            Func<object, IComparisonValidator> ctor1Fun,
            Func<Func<object, object>, MemberInfo, IComparisonValidator> ctor2Fun)
            where T : ComparisonAttribute
        {
            if (attr == null)
            {
                return null;
            }

            IComparisonValidator propValidator = null;
            if (attr.CompareToValue != null)
            {
                return ctor1Fun(attr.CompareToValue);
            }

            if (attr.ComparsionProperty != null)
            {
                var propInfo = type.GetProperty(attr.ComparsionProperty);
                if (propInfo == null)
                {
                    throw new ArgumentException($"ComparisonProperty '{attr.ComparsionProperty}' of {typeof(T)} in type '{type}' was not found");
                }

                Func<object, object> fun = propInfo.GetValue;
                propValidator = ctor2Fun(fun, propInfo);
            }

            return propValidator;

            //AddAttributePropertyValidator(propValidator, prop, attr.IncludePropertyName);
        }

        private void AddAttributePropertyValidator(IPropertyValidator propValidator, PropertyInfo prop, bool includePropertyName)
        {
            AddPropertyValidator(propValidator, prop, ValidationRuleSet.Attribute, includePropertyName);
        }

        public void AddPropertyValidator(IPropertyValidator propValidator, PropertyInfo prop, string ruleSet, bool includePropertyName)
        {
            var rule = CreateRule(typeof(TModel), prop.Name);
            rule.RuleSets = new [] { ruleSet};
            rule.AddValidator(propValidator);
            var message = propValidator.GetMessageId(includePropertyName);
            rule.MessageBuilder = ctx => ctx.MessageFormatter.BuildMessage(message);
            AddRule(rule);
        }

        private static PropertyRule CreateRule(Type type, string propName)
        {
            var p = Expression.Parameter(type);
            var body = Expression.Property(p, propName);
            var expr = Expression.Lambda(body, p);

            var propInfo = body.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new NullReferenceException("propInfo");
            }

            var createRuleMethod = typeof(PropertyRule)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select(o => o.MakeGenericMethod(type, propInfo.PropertyType))
                .First(o => o.Name == "Create" && o.GetParameters().Count() == 1);

            return (PropertyRule)createRuleMethod.Invoke(null, new object[] { expr });
        }
    }
}
