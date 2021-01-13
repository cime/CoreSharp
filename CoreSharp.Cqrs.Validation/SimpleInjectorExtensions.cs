using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;
using CoreSharp.Cqrs.Validation;

namespace SimpleInjector
{
    public static class SimpleInjectorExtensions
    {

        public static Container AddQueriesValidation(this Container container)
        {
            container.RegisterDecorator(typeof(IAsyncQueryHandler<,>), typeof(ValidateAsyncQueryDecorator<,>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(ValidateQueryDecorator<,>), Lifestyle.Transient);
            return container;
        }

        public static Container AddCommandsValidation(this Container container)
        {
            container.RegisterDecorator(typeof(IAsyncCommandHandler<>), typeof(ValidateAsyncCommandDecorator<>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(IAsyncCommandHandler<,>), typeof(ValidateAsyncCommandDecorator<,>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ValidateCommandDecorator<>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(ICommandHandler<,>), typeof(ValidateCommandDecorator<,>), Lifestyle.Transient);
            return container;
        }
    }
}
