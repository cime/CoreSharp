using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Query;
using CoreSharp.Validation;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Cqrs.Validation
{
    public class ValidateAsyncQueryDecorator<TQuery, TResult> : IAsyncQueryHandler<TQuery, TResult>
        where TQuery : IAsyncQuery<TResult>
    {
        private readonly Container _container;
        private readonly IAsyncQueryHandler<TQuery, TResult> _decorated;

        public ValidateAsyncQueryDecorator(Container container, IAsyncQueryHandler<TQuery, TResult> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var validator = (IValidator<TQuery>)((IServiceProvider)_container).GetService(typeof(IValidator<TQuery>));
            if (validator != null)
            {
                await ValidateUtil.ValidateAsync(async () =>
                {
                    await validator.ValidateAndThrowAsync(query, ValidationRuleSet.InsertUpdate);
                });     
            }

            return await _decorated.HandleAsync(query, cancellationToken);
        }
    }
}
