using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using CoreSharp.Validation;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Cqrs.Validation
{

    public abstract class ValidateAsyncCommandDecorator
    {
        protected async Task ValidateAndThrowAsync<TCommand>(Container _container, TCommand command)
        {
            // try with fluent validator
            var validator = (IValidator<TCommand>)((IServiceProvider)_container).GetService(typeof(IValidator<TCommand>));
            if (validator != null)
            {
                await ValidateUtil.ValidateAsync(async () =>
                {
                    await validator.ValidateAndThrowAsync(command, ValidationRuleSet.InsertUpdate);
                });                
            }
        }
    }

    public class ValidateAsyncCommandDecorator<TCommand> : ValidateAsyncCommandDecorator, IAsyncCommandHandler<TCommand>
        where TCommand : IAsyncCommand
    {
        private readonly Container _container;
        private readonly IAsyncCommandHandler<TCommand> _decorated;

        public ValidateAsyncCommandDecorator(Container container, IAsyncCommandHandler<TCommand> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await ValidateUtil.ValidateAsync(async () =>
            {
                await ValidateAndThrowAsync(_container, command);
            });
            await _decorated.HandleAsync(command, cancellationToken);
        }
    }

    public class ValidateAsyncCommandDecorator<TCommand, TReturn> : ValidateAsyncCommandDecorator, IAsyncCommandHandler<TCommand, TReturn>
        where TCommand : IAsyncCommand<TReturn>
    {
        private readonly Container _container;
        private readonly IAsyncCommandHandler<TCommand, TReturn> _decorated;

        public ValidateAsyncCommandDecorator(Container container, IAsyncCommandHandler<TCommand, TReturn> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public async Task<TReturn> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await ValidateUtil.ValidateAsync(async () =>
            {
                await ValidateAndThrowAsync(_container, command);
            });
            return await _decorated.HandleAsync(command, cancellationToken);
        }
    }
}
