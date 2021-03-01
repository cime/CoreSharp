using System;
using CoreSharp.Cqrs.Query;
using CoreSharp.Validation;
using CoreSharp.Validation.Abstractions;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Cqrs.Validation
{
    public class ValidateQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly Container _container;
        private readonly IQueryHandler<TQuery, TResult> _decorated;

        public ValidateQueryDecorator(Container container, IQueryHandler<TQuery, TResult> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public TResult Handle(TQuery query)
        {
            var validator = (IValidator<TQuery>)((IServiceProvider)_container).GetService(typeof(IValidator<TQuery>));
            if (validator != null)
            {
                ValidateUtil.Validate(() =>
                {
                    validator.ValidateAndThrow(query, ValidationRuleSet.InsertUpdate);
                });
            }

            return _decorated.Handle(query);
        }
    }
}
