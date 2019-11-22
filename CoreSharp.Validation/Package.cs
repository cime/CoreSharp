using System;
using CoreSharp.Validation.Internal;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Validation
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<ValidatorCache>();

            var registration = Lifestyle.Singleton.CreateRegistration<ValidatorFactory>(container);
            container.AddRegistration(typeof(IValidatorFactory), registration);
            container.RegisterDecorator(typeof(IValidator<>), typeof(ValidatorDecorator<>), Lifestyle.Singleton);
            container.ResolveUnregisteredType += (sender, e) => TryRegisterValidator(container, e);

            ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory = strings => new CustomRulesetValidatorSelector(strings);
            ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory = () => new CustomRulesetValidatorSelector();
        }

        private static void TryRegisterValidator(Container container, UnregisteredTypeEventArgs e)
        {
            // Register only IValidator<> interfaces, registering interfaces that extends IValidator<> is not an option as
            // Validator<TModel> does not implement them
            if (!e.UnregisteredServiceType.IsGenericType || 
                e.UnregisteredServiceType.GetGenericTypeDefinition() != typeof(IValidator<>))
            {
                return;
            }

            var validatorType = e.UnregisteredServiceType.GetGenericType(typeof(IValidator<>));
            if (validatorType == null)
            {
                return;
            }

            var concreteType = typeof(Validator<>).MakeGenericType(validatorType.GenericTypeArguments[0]);
            e.Register(Lifestyle.Singleton.CreateRegistration(concreteType, container));
        }
    }
}
