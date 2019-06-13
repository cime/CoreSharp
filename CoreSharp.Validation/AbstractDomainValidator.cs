using System;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public abstract class AbstractDomainValidator<TModel> : IDomainValidator<TModel>
    {
        public ValidationFailure Validate(object model, ValidationContext context)
        {
            return Validate((TModel) model, context);
        }

        public bool CanValidate(object model, ValidationContext context)
        {
            return CanValidate((TModel)model, context);
        }

        public string[] RuleSets { get; set; }

        public abstract ValidationFailure Validate(TModel model, ValidationContext context);

        public abstract bool CanValidate(TModel model, ValidationContext context);

        protected ValidationFailure Failure(Expression<Func<TModel, object>> propertyExp, string errorMessage)
        {
            return new ValidationFailure(propertyExp.GetFullPropertyName(), errorMessage);
        }

        protected ValidationFailure Failure(string errorMessage)
        {
            return new ValidationFailure("", errorMessage);
        }

        protected ValidationFailure Success => null;
    }
}
