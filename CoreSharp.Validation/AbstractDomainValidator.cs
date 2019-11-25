using System;
using System.Linq.Expressions;
using CoreSharp.Validation.Internal;
using FluentValidation;
using FluentValidation.Results;

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

        protected ValidationFailure Failure<TProperty>(Expression<Func<TChild, TProperty>> propertyExp, string errorMessage, ValidationContext context)
        {
            return DomainValidationContext.CreateValidationFailure(propertyExp, errorMessage, context);
        }

        protected ValidationFailure Failure(string errorMessage, ValidationContext context)
        {
            return DomainValidationContext.CreateValidationFailure(errorMessage, context);
        }

        protected ValidationFailure Success => null;
    }
}
