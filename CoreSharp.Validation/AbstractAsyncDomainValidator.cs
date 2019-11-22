using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public abstract class AbstractAsyncDomainValidator<TModel> : AbstractAsyncDomainValidator<TModel, TModel>
    {
    }

    public abstract class AbstractAsyncDomainValidator<TRoot, TChild> : IAsyncDomainValidator<TRoot, TChild>
    {
        public virtual string[] RuleSets { get; }

        public abstract Task<ValidationFailure> ValidateAsync(TChild model, ValidationContext context);

        public abstract Task<bool> CanValidateAsync(TChild model, ValidationContext context);

        public virtual Task BeforeValidationAsync(TRoot root, ValidationContext context)
        {
            return Task.CompletedTask;
        }

        Task<ValidationFailure> IAsyncDomainValidator.ValidateAsync(object model, ValidationContext context)
        {
            return ValidateAsync((TChild)model, context);
        }

        Task<bool> IAsyncDomainValidator.CanValidateAsync(object model, ValidationContext context)
        {
            return CanValidateAsync((TChild)model, context);
        }

        Task IAsyncDomainValidator.BeforeValidationAsync(object model, ValidationContext context)
        {
            return BeforeValidationAsync((TRoot)model, context);
        }

        protected ValidationFailure Failure(Expression<Func<TChild, object>> propertyExp, string errorMessage, ValidationContext context)
        {
            var attemptedValue = context.InstanceToValidate is TChild child
                ? propertyExp.Compile()(child)
                : null;

            return new ValidationFailure(propertyExp.GetFullPropertyName(), errorMessage, attemptedValue);
        }

        protected ValidationFailure Failure(string errorMessage, ValidationContext context)
        {
            return new ValidationFailure(context.PropertyChain.ToString(), errorMessage, context.InstanceToValidate);
        }

        protected ValidationFailure Success => null;
    }
}
