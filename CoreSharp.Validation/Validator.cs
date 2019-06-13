using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.Validation.Attributes;
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
                    IPropertyValidator propValidator = null;
                    #region NotNullAttribute

                    if (attr is NotNullAttribute)
                    {
                        propValidator = new NotNullValidator();
                    }

                    #endregion
                    #region EmailAttribute

                    if (attr is EmailAttribute)
                    {
                        propValidator = new EmailValidator();
                    }

                    #endregion
                    #region EnumAttribute

                    var enumAttr = attr as EnumAttribute;
                    if (enumAttr != null)
                    {
                        propValidator = new EnumValidator(enumAttr.Type);
                    }

                    #endregion
                    #region NotEmptyAttribute

                    var notEmptyAttr = attr as NotEmptyAttribute;
                    if (notEmptyAttr != null)
                    {
                        propValidator = new NotEmptyValidator(notEmptyAttr.DefaultValue ?? prop.PropertyType.GetDefaultValue());
                    }

                    #endregion
                    #region NotNullOrEmptyAttribute

                    var notNullOrEmptyAttr = attr as NotNullOrEmptyAttribute;
                    if (notNullOrEmptyAttr != null)
                    {
                        propValidator = new NotEmptyValidator(prop.PropertyType.GetDefaultValue());
                        AddAttributePropertyValidator(new NotNullValidator(), prop, attr.IncludePropertyName);
                    }

                    #endregion
                    #region EqualAttribute

                    AddComparisonValidator(attr as EqualAttribute, type, prop,
                        o => new EqualValidator(o),
                        (func, info) => new EqualValidator(func, info));

                    #endregion
                    #region LengthAttribute

                    var lengthAttr = attr as LengthAttribute;
                    if (lengthAttr != null)
                    {
                        propValidator = new LengthValidator(lengthAttr.Min, lengthAttr.Max);
                    }

                    #endregion
                    #region NotEqualAttribute

                    AddComparisonValidator(attr as NotEqualAttribute, type, prop,
                        o => new NotEqualValidator(o),
                        (func, info) => new NotEqualValidator(func, info));

                    #endregion
                    #region RegularExpressionAttribute

                    var regexAttr = attr as RegularExpressionAttribute;
                    if (regexAttr != null)
                    {
                        propValidator = new RegularExpressionValidator(regexAttr.Expression, regexAttr.RegexOptions);
                    }

                    #endregion
                    #region CreditCardAttribute

                    if (attr is CreditCardAttribute)
                    {
                        propValidator = new CreditCardValidator();
                    }

                    #endregion
                    #region ScalePrecisionAttribute

                    var scalePrecisionAttr = attr as ScalePrecisionAttribute;
                    if (scalePrecisionAttr != null)
                    {
                        propValidator = new ScalePrecisionValidator(scalePrecisionAttr.Scale, scalePrecisionAttr.Precision);
                    }

                    #endregion
                    #region ExactLengthAttribute

                    var exctLenAttr = attr as ExactLengthAttribute;
                    if (exctLenAttr != null)
                    {
                        propValidator = new ExactLengthValidator(exctLenAttr.Length);
                    }

                    #endregion
                    #region ExclusiveBetweenAttribute

                    var exclusiveBetweenAttribute = attr as ExclusiveBetweenAttribute;
                    if (exclusiveBetweenAttribute != null)
                    {
                        if (exclusiveBetweenAttribute.From != null && exclusiveBetweenAttribute.To != null &&
                            exclusiveBetweenAttribute.From.GetType() != exclusiveBetweenAttribute.To.GetType())
                        {
                            var fromConverted = Convert.ChangeType(exclusiveBetweenAttribute.From, exclusiveBetweenAttribute.To.GetType());

                            propValidator = new ExclusiveBetweenValidator(fromConverted as IComparable, exclusiveBetweenAttribute.To as IComparable);
                        }
                        else
                        {
                            propValidator = new ExclusiveBetweenValidator(exclusiveBetweenAttribute.From as IComparable, exclusiveBetweenAttribute.To as IComparable);
                        }
                    }

                    #endregion
                    #region ExclusiveBetweenAttribute

                    var inclusiveBetweenAttribute = attr as InclusiveBetweenAttribute;
                    if (inclusiveBetweenAttribute != null)
                    {
                        if (inclusiveBetweenAttribute.From != null && inclusiveBetweenAttribute.To != null &&
                            inclusiveBetweenAttribute.From.GetType() != inclusiveBetweenAttribute.To.GetType())
                        {
                            var fromConverted = Convert.ChangeType(inclusiveBetweenAttribute.From, inclusiveBetweenAttribute.To.GetType());

                            propValidator = new InclusiveBetweenValidator(fromConverted as IComparable, inclusiveBetweenAttribute.To as IComparable);
                        }
                        else
                        {
                            propValidator = new InclusiveBetweenValidator(inclusiveBetweenAttribute.From as IComparable, inclusiveBetweenAttribute.To as IComparable);
                        }
                    }

                    #endregion
                    #region GreaterThanAttribute

                    AddComparisonValidator(attr as GreaterThanAttribute, type, prop,
                        o => new GreaterThanValidator(o as IComparable),
                        (func, info) => new GreaterThanValidator(func, info));

                    #endregion
                    #region GreaterThanOrEqualAttribute

                    AddComparisonValidator(attr as GreaterThanOrEqualAttribute, type, prop,
                        o => new GreaterThanOrEqualValidator(o as IComparable),
                        (func, info) => new GreaterThanOrEqualValidator(func, info));

                    #endregion
                    #region LessThanOrEqualAttribute

                    AddComparisonValidator(attr as LessThanOrEqualAttribute, type, prop,
                        o => new LessThanOrEqualValidator(o as IComparable),
                        (func, info) => new LessThanOrEqualValidator(func, info));

                    #endregion
                    #region LessThanAttribute

                    AddComparisonValidator(attr as LessThanAttribute, type, prop,
                        o => new LessThanValidator(o as IComparable),
                        (func, info) => new LessThanValidator(func, info));

                    #endregion

                    if (propValidator == null)
                    {
                        continue;
                    }

                    AddAttributePropertyValidator(propValidator, prop, attr.IncludePropertyName);
                }
            }
        }

        private void AddComparisonValidator<T>(T attr, Type type, PropertyInfo prop, Func<object, IComparisonValidator> ctor1Fun,
            Func<Func<object, object>, MemberInfo, IComparisonValidator> ctor2Fun)
            where T : ComparisonAttribute
        {
            if (attr == null) return;
            IComparisonValidator propValidator = null;

            if (attr.CompareToValue != null)
            {
                propValidator = ctor1Fun(attr.CompareToValue);
            }
            if (attr.ComparsionProperty != null)
            {
                if (propValidator != null)
                    AddAttributePropertyValidator(propValidator, prop, attr.IncludePropertyName);

                var propInfo = type.GetProperty(attr.ComparsionProperty);
                if (propInfo == null)
                    throw new ArgumentException(string.Format("ComparisonProperty '{0}' of {1} in type '{2}' was not found",
                        attr.ComparsionProperty, typeof(T), type));

                Func<object, object> fun = propInfo.GetValue;
                propValidator = ctor2Fun(fun, propInfo);
            }
            AddAttributePropertyValidator(propValidator, prop, attr.IncludePropertyName);
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
            //rule.SetL10NMessage(includePropertyName);
            AddRule(rule);
        }

        private static PropertyRule CreateRule(Type type, string propName)
        {
            var p = Expression.Parameter(type);
            var body = Expression.Property(p, propName);
            var expr = Expression.Lambda(body, p);

            var propInfo = body.Member as PropertyInfo;
            if (propInfo == null)
                throw new NullReferenceException("propInfo");

            var createRuleMethod = typeof(PropertyRule)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select(o => o.MakeGenericMethod(type, propInfo.PropertyType))
                .First(o => o.Name == "Create" && o.GetParameters().Count() == 1);

            return (PropertyRule)createRuleMethod.Invoke(null, new object[] { expr });
        }
    }
}
