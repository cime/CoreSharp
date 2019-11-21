using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public abstract class AbstractAsyncDomainValidator<TModel> : IAsyncDomainValidator<TModel>
    {
        public Task<ValidationFailure> ValidateAsync(object model, ValidationContext context)
        {
            return ValidateAsync((TModel) model, context);
        }

        public Task<bool> CanValidateAsync(object model, ValidationContext context)
        {
            return CanValidateAsync((TModel)model, context);
        }

        public string[] RuleSets { get; set; }

        public abstract Task<ValidationFailure> ValidateAsync(TModel model, ValidationContext context);

        public abstract Task<bool> CanValidateAsync(TModel model, ValidationContext context);

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
