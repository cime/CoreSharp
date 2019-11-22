using FluentValidation;
using CoreSharp.Tests.Validation.Models;
using SimpleInjector;
using Xunit;
using CoreSharp.Common.Tests;

namespace CoreSharp.Tests.Validation
{
    public class LifecycleTests : IClassFixture<Bootstrapper>
    {
        private Bootstrapper _baseTest;

        public LifecycleTests(Bootstrapper baseTest)
        {
            _baseTest = baseTest;
            _baseTest.BeforeInitialization(container =>
            {
                container.RegisterValidatorsFromAssemblyOf<LifecycleTests>();
                container.RegisterPackages();
            });

            _baseTest.Initialize();
        }

        [Fact]
        public void ValidatorMustBeSingleton()
        {
            var validatorFactory = _baseTest.Container.GetInstance<IValidatorFactory>();
            var validator = validatorFactory.GetValidator(typeof(TestModel));
            var validator2 = validatorFactory.GetValidator<TestModel>();
            var validator3 = _baseTest.Container.GetInstance<IValidator<TestModel>>();
            var validator4 = _baseTest.Container.GetInstance<IValidator<TestModel>>();
            
            Assert.Equal(validator, validator2);
            Assert.Equal(validator2, validator3);
            Assert.Equal(validator3, validator4);
        }

        [Fact]
        public void CustomValidatorInterfaceShouldNotBeRegistered()
        {
            Assert.Throws<ActivationException>(() =>
            {
                _baseTest.Container.GetInstance<ITestModelValidator>();
            });
        }

        [Fact]
        public void GenericValidatorMustBeSingleton()
        {
            var validatorFactory = _baseTest.Container.GetInstance<IValidatorFactory>();

            var validator = _baseTest.Container.GetInstance<IValidator<SubChild>>();
            var validator2 = _baseTest.Container.GetInstance<IValidator<SubChild>>();
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
            var validatorFactory = _baseTest.Container.GetInstance<IValidatorFactory>();
            var validatorFactory2 = _baseTest.Container.GetInstance<IValidatorFactory>();

            Assert.Equal(validatorFactory, validatorFactory2);
        }

        [Fact]
        public void UnregisteredValidatorMustBeSingleton()
        {
            var validator = _baseTest.Container.GetInstance<IValidator<LifecycleTests>>();
            var validator2 = _baseTest.Container.GetInstance<IValidator<LifecycleTests>>();

            Assert.Equal(validator, validator2);
        }
    }

}
