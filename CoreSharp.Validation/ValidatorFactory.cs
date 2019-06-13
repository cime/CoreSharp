using System;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Validation
{
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly Container _container;

        /// <summary>The constructor of the factory.</summary>
        /// <param name="container">The Simple Injector Container</param>
        public ValidatorFactory(Container container)
        {
            _container = container;
        }

        /// <summary>Gets the validator for the specified type.</summary>
        public IValidator<T> GetValidator<T>()
        {
            var validator = typeof(IValidator<>).MakeGenericType(typeof(T));

            return (IValidator<T>)((IServiceProvider)_container).GetService(validator);
        }

        /// <summary>Gets the validator for the specified type.</summary>
        public IValidator GetValidator(Type type)
        {
            var validator = typeof(IValidator<>).MakeGenericType(type);

            return (IValidator)((IServiceProvider)_container).GetService(validator);
        }
    }
}
