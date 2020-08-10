using CoreSharp.Common.Tests;
using CoreSharp.Validation.Tests.Models;
using FluentValidation;
using SimpleInjector;
using Xunit;

namespace CoreSharp.Validation.Tests
{
    public class LifecycleTests : BaseTest
    {
        public LifecycleTests(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        protected override void ConfigureContainer(Container container)
        {
            container.RegisterValidatorsFromAssemblyOf<LifecycleTests>();
        }

        [Fact]
        public void ValidatorMustBeSingleton()
        {
            var validatorFactory = Container.GetInstance<IValidatorFactory>();
            var validator = validatorFactory.GetValidator(typeof(TestModel));
            var validator2 = validatorFactory.GetValidator<TestModel>();
            var validator3 = Container.GetInstance<IValidator<TestModel>>();
            var validator4 = Container.GetInstance<IValidator<TestModel>>();
            
            Assert.Equal(validator, validator2);
            Assert.Equal(validator2, validator3);
            Assert.Equal(validator3, validator4);
        }

        [Fact]
        public void CustomValidatorInterfaceShouldNotBeRegistered()
        {
            Assert.Throws<ActivationException>(() =>
            {
                Container.GetInstance<ITestModelValidator>();
            });
        }

        [Fact]
        public void GenericValidatorMustBeSingleton()
        {
            var validatorFactory = Container.GetInstance<IValidatorFactory>();

            var validator = Container.GetInstance<IValidator<SubChild>>();
            var validator2 = Container.GetInstance<IValidator<SubChild>>();
            var validator3 = validatorFactory.GetValidator(typeof(SubChild));
            var validator4 = validatorFactory.GetValidator<SubChild>();

            Assert.Equal("ValidatorDecorator`1", validator.GetType().Name);
            Assert.Equal(validator, validator2);
            Assert.Equal(validator2, validator3);
            Assert.Equal(validator3, validator4);
        }

        [Fact]
        public void ValidatorFactoryMustBeSingleton()
        {
            var validatorFactory = Container.GetInstance<IValidatorFactory>();
            var validatorFactory2 = Container.GetInstance<IValidatorFactory>();

            Assert.Equal(validatorFactory, validatorFactory2);
        }

        [Fact]
        public void UnregisteredValidatorMustBeSingleton()
        {
            var validator = Container.GetInstance<IValidator<LifecycleTests>>();
            var validator2 = Container.GetInstance<IValidator<LifecycleTests>>();

            Assert.Equal(validator, validator2);
        }
    }

}
