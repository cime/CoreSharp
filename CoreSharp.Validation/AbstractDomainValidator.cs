using System;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using static CoreSharp.Common.Internationalization.I18N;

namespace CoreSharp.Validation
{
    public abstract class AbstractDomainValidator<TModel> : AbstractDomainValidator<TModel, TModel>
    {
    }

    public abstract class AbstractDomainValidator<TRoot, TChild> : IDomainValidator<TRoot, TChild>
    {
        public virtual string[] RuleSets { get; }

        public abstract ValidationFailure Validate(TChild model, ValidationContext context);

        public virtual bool CanValidate(TChild model, ValidationContext context)
        {
            return true;
        }

        public virtual void BeforeValidation(TRoot root, ValidationContext context)
        {
        }

        ValidationFailure IDomainValidator.Validate(object model, ValidationContext context)
        {
            return Validate((TChild)model, context);
        }

        bool IDomainValidator.CanValidate(object model, ValidationContext context)
        {
            return CanValidate((TChild)model, context);
        }

        void IDomainValidator.BeforeValidation(object model, ValidationContext context)
        {
            BeforeValidation((TRoot)model, context);
        }

        protected ValidationFailure Failure(Expression<Func<TChild, object>> propertyExp, string errorMessage, ValidationContext context)
        {
            var attemptedValue = context.InstanceToValidate is TChild child
                ? propertyExp.Compile()(child)
                : null;

            return new ValidationFailure(propertyExp.GetFullPropertyName(), _(errorMessage), attemptedValue);
        }

        protected ValidationFailure Failure(string errorMessage, ValidationContext context)
        {
            return new ValidationFailure(context.PropertyChain.ToString(), _(errorMessage), context.InstanceToValidate);
        }

        protected ValidationFailure Success => null;
    }
}
