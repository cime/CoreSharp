using System;
using CoreSharp.Cqrs.Command;
using CoreSharp.Validation;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Cqrs.Validation
{
    public class ValidateCommandDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly Container _container;
        private readonly ICommandHandler<TCommand> _decorated;

        public ValidateCommandDecorator(Container container, ICommandHandler<TCommand> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public void Handle(TCommand command)
        {
            var validator = (IValidator<TCommand>)((IServiceProvider)_container).GetService(typeof(IValidator<TCommand>));
            if (validator != null)
            {
                ValidateUtil.Validate(() =>
                {
                    validator.ValidateAndThrow(command, ValidationRuleSet.InsertUpdate);
                });
            }

            _decorated.Handle(command);
        }
    }

    public class ValidateCommandDecorator<TCommand, TReturn> : ICommandHandler<TCommand, TReturn>
        where TCommand : ICommand<TReturn>
    {
        private readonly Container _container;
        private readonly ICommandHandler<TCommand, TReturn> _decorated;

        public ValidateCommandDecorator(Container container, ICommandHandler<TCommand, TReturn> decorated)
        {
            _container = container;
            _decorated = decorated;
        }

        public TReturn Handle(TCommand command)
        {
            var validator = (IValidator<TCommand>)((IServiceProvider)_container).GetService(typeof(IValidator<TCommand>));
            if (validator != null)
            {
                ValidateUtil.Validate(() =>
                {
                    validator.ValidateAndThrow(command, ValidationRuleSet.InsertUpdate);
                });
            }

            return _decorated.Handle(command);
        }
    }
}
