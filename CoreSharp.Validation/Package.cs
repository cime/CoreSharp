using CoreSharp.Validation.Internal;
using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Validation
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<IValidatorFactory, ValidatorFactory>(Lifestyle.Singleton);
            container.RegisterConditional(typeof(IValidator<>), typeof(Validator<>), Lifestyle.Singleton, o => !o.Handled);
            container.RegisterDecorator(typeof(IValidator<>), typeof(ValidatorDecorator<>), Lifestyle.Singleton);

            ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory = strings => new CustomRulesetValidatorSelector(strings);
            ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory = () => new CustomRulesetValidatorSelector();
        }
    }
}
