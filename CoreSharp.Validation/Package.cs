using FluentValidation;
using SimpleInjector;

namespace CoreSharp.Validation
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<IValidatorFactory, ValidatorFactory>(Lifestyle.Singleton);
        }
    }
}
