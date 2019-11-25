using System;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using static CoreSharp.Common.Internationalization.I18N;

namespace CoreSharp.Validation.Internal
{
    internal class DomainValidationContext : IValidationContext
    {
        public DomainValidationContext(object instanceToValdiate, object propertyValue, IValidationContext parentContext)
        {
            InstanceToValidate = instanceToValdiate;
            PropertyValue = propertyValue;
            ParentContext = parentContext;
        }

        public object InstanceToValidate { get; }

        public object PropertyValue { get; }

        public IValidationContext ParentContext { get; }

        internal static ValidationFailure CreateValidationFailure<TModel, TProperty>(Expression<Func<TModel, TProperty>> propertyExp, string errorMessage, ValidationContext context)
        {
            if (!(propertyExp.Body is MemberExpression memberExpression) || memberExpression.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new InvalidOperationException("Invalid property expression");
            }

            var newContext = context.Clone();
            newContext.PropertyChain.Add(memberExpression.Member);
            var attemptedValue = context.InstanceToValidate is TModel child
                ? propertyExp.Compile()(child)
                : (object)null;

            return new ValidationFailure(propertyExp.GetFullPropertyName(), errorMessage, attemptedValue)
            {
                CustomState = new DomainValidationContext(context.InstanceToValidate, attemptedValue, context)
            };
        }

        internal static ValidationFailure CreateValidationFailure(string errorMessage, ValidationContext context)
        {
            var propName = context.PropertyChain.ToString();
            return new ValidationFailure(string.IsNullOrEmpty(propName) ? null : propName, _(errorMessage), context.InstanceToValidate)
            {
                CustomState = context
            };
        }
    }
}
