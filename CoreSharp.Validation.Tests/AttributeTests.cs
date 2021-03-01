using CoreSharp.Common.Tests;
using CoreSharp.Validation.Tests.Models.Attributes;
using FluentValidation;
using SimpleInjector;
using Xunit;

namespace CoreSharp.Validation.Tests
{
    public class AttributeTests : BaseTest
    {
        public AttributeTests(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        protected override void ConfigureContainer(Container container)
        {
            container.RegisterValidatorsFromAssemblyOf<LifecycleTests>();
        }

        [Fact]
        public void Length()
        {
            var validator = Container.GetInstance<IValidator<LengthModel>>();

            var model = new LengthModel
            {
                Nickname = "Test",
                Nickname2 = "Test",
                Name = "Test",
                Name2 = "Test"
            };
            var result = validator.Validate(model, ruleSet: ValidationRuleSet.Attribute);
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Nickname", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be between 5 and 10 characters. You entered 4 characters.", result.Errors[0].ErrorMessage);
            Assert.Equal("Nickname2", result.Errors[1].PropertyName);
            Assert.Equal("'Nickname2' must be between 5 and 10 characters. You entered 4 characters.", result.Errors[1].ErrorMessage);

            model = new LengthModel
            {
                Nickname = "Test2",
                Nickname2 = "Test2",
                Name = "Test012345679",
                Name2 = "Test012345679"
            };
            result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.Equal("The length must be 10 characters or fewer. You entered 13 characters.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("The length of 'Name2' must be 10 characters or fewer. You entered 13 characters.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void NotNull()
        {
            var validator = Container.GetInstance<IValidator<NotNullModel>>();

            var model = new NotNullModel();
            var result = validator.Validate(model, ruleSet: ValidationRuleSet.Attribute);
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must not be empty.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("'Name2' must not be empty.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void CreditCard()
        {
            var validator = Container.GetInstance<IValidator<CreditCardModel>>();

            var model = new CreditCardModel
            {
                CardNumber = "35ff54854",
                CardNumber2 = "3554f854"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("CardNumber", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Is not a valid credit card number.", result.Errors[0].ErrorMessage);
            Assert.Equal("CardNumber2", result.Errors[1].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[1].CustomState);
            Assert.Equal("'Card Number2' is not a valid credit card number.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void Email()
        {
            var validator = Container.GetInstance<IValidator<EmailModel>>();

            var model = new EmailModel
            {
                Email = "invalid@email",
                Email2 = "invalid@email"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Email", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Is not a valid email address.", result.Errors[0].ErrorMessage);
            Assert.Equal("Email2", result.Errors[1].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[1].CustomState);
            Assert.Equal("'Email2' is not a valid email address.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void Equal()
        {
            var validator = Container.GetInstance<IValidator<EqualModel>>();

            var model = new EqualModel
            {
                Name = "Jon",
                Name2 = "Jon",
                LastName = "Ventura",
                LastName2 = "Ventura"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be equal to 'CompareValue'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("'Name2' must be equal to 'CompareValue'.", result.Errors[1].ErrorMessage);
            Assert.Equal("LastName", result.Errors[2].PropertyName);
            Assert.Equal("Must be equal to 'LastNameCompare'.", result.Errors[2].ErrorMessage);
            Assert.Equal("LastName2", result.Errors[3].PropertyName);
            Assert.Equal("'Last Name2' must be equal to 'LastNameCompare'.", result.Errors[3].ErrorMessage);
        }

        [Fact]
        public void NotEqual()
        {
            var validator = Container.GetInstance<IValidator<NotEqualModel>>();

            var model = new NotEqualModel
            {
                Name = "CompareValue",
                Name2 = "CompareValue",
                LastName = "LastNameCompare",
                LastName2 = "LastNameCompare"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must not be equal to 'CompareValue'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("'Name2' must not be equal to 'CompareValue'.", result.Errors[1].ErrorMessage);
            Assert.Equal("LastName", result.Errors[2].PropertyName);
            Assert.Equal("Must not be equal to 'LastNameCompare'.", result.Errors[2].ErrorMessage);
            Assert.Equal("LastName2", result.Errors[3].PropertyName);
            Assert.Equal("'Last Name2' must not be equal to 'LastNameCompare'.", result.Errors[3].ErrorMessage);
        }

        [Fact]
        public void ExactLength()
        {
            var validator = Container.GetInstance<IValidator<ExactLengthModel>>();

            var model = new ExactLengthModel
            {
                Name = "Ann",
                Name2 = "Ann"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be 5 characters in length. You entered 3 characters.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("'Name2' must be 5 characters in length. You entered 3 characters.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void GreaterThan()
        {
            var validator = Container.GetInstance<IValidator<GreaterThanModel>>();

            var model = new GreaterThanModel
            {
                Value = 10,
                Value2 = 10
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Value", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be greater than '10'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Value2", result.Errors[1].PropertyName);
            Assert.Equal("'Value2' must be greater than '10'.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void GreaterThanOrEqual()
        {
            var validator = Container.GetInstance<IValidator<GreaterThanOrEqualModel>>();

            var model = new GreaterThanOrEqualModel
            {
                Value = 9,
                Value2 = 9
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Value", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be greater than or equal to '10'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Value2", result.Errors[1].PropertyName);
            Assert.Equal("'Value2' must be greater than or equal to '10'.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void LessThan()
        {
            var validator = Container.GetInstance<IValidator<LessThanModel>>();

            var model = new LessThanModel
            {
                Value = 10,
                Value2 = 10
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Value", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be less than '10'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Value2", result.Errors[1].PropertyName);
            Assert.Equal("'Value2' must be less than '10'.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void LessThanOrEqual()
        {
            var validator = Container.GetInstance<IValidator<LessThanOrEqualModel>>();

            var model = new LessThanOrEqualModel
            {
                Value = 11,
                Value2 = 11
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Value", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Must be less than or equal to '10'.", result.Errors[0].ErrorMessage);
            Assert.Equal("Value2", result.Errors[1].PropertyName);
            Assert.Equal("'Value2' must be less than or equal to '10'.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void RegularExpression()
        {
            var validator = Container.GetInstance<IValidator<RegularExpressionModel>>();

            var model = new RegularExpressionModel
            {
                Name = "test",
                Name2 = "test"
            };
            var result = validator.Validate(model, ruleSets: new[] { ValidationRuleSet.Attribute });
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Name", result.Errors[0].PropertyName);
            Assert.IsAssignableFrom<IValidationContext>(result.Errors[0].CustomState);
            Assert.Equal("Is not in the correct format.", result.Errors[0].ErrorMessage);
            Assert.Equal("Name2", result.Errors[1].PropertyName);
            Assert.Equal("'Name2' is not in the correct format.", result.Errors[1].ErrorMessage);
        }

        [Fact]
        public void IgnoreValidationAttributes()
        {
            var validator = Container.GetInstance<IValidator<IgnoreValidationAttributesModel>>();

            var model = new IgnoreValidationAttributesModel();
            var result = validator.Validate(model, ruleSet: ValidationRuleSet.Attribute);
            Assert.True(result.IsValid);
        }
    }
}
