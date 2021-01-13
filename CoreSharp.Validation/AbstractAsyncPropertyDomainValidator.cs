using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CoreSharp.Validation;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public abstract class AbstractAsyncPropertyDomainValidator<TModel, TProp> : AbstractAsyncDomainValidator<TModel>
    {
        public override async Task<ValidationFailure> ValidateAsync(TModel model, ValidationContext context)
        {
            var propGetter = PropertyExpression.Compile();
            var propVal = propGetter(model);


            bool valid = false;
            try
            {
                valid = await IsValid(model, propVal);
            }
            catch (Exception)
            {
            }

            if (!valid)
            {
                return Failure(PropertyExpression, ErrorTemplate, context);
            }

            return Success;
        }

        protected abstract string ErrorTemplate { get; }

        protected abstract Task<bool> IsValid(TModel model, TProp propValue);

        protected abstract Expression<Func<TModel, TProp>> PropertyExpression { get; }

    }
}
