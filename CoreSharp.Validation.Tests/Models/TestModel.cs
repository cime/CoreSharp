using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation.Tests.Models
{
    public class TestModel
    {
        public string Name { get; set; }
    }

    public interface ITestModelValidator : IValidator<TestModel>
    {
    }

    public class TestModelValidator : Validator<TestModel>, ITestModelValidator
    {
    }

    public class TestModelDomainValidator : AbstractDomainValidator<TestModel>
    {
        public static int ValidateCount;
        public static int CanValidateCount;
        public static int ValidateBeforeValidationCount;

        public override void BeforeValidation(TestModel root, ValidationContext context)
        {
            ValidateBeforeValidationCount++;
        }

        public override IEnumerable<ValidationFailure> Validate(TestModel child, ValidationContext context)
        {
            ValidateCount++;

            yield break;
        }

        public override bool CanValidate(TestModel child, ValidationContext context)
        {
            CanValidateCount++;
            return true;
        }

        public override string[] RuleSets => new string[] {};
    }

    public class TestModelAsyncDomainValidator : AbstractAsyncDomainValidator<TestModel>
    {
        public static int ValidateCount;
        public static int CanValidateCount;
        public static int ValidateBeforeValidationCount;

        public override Task BeforeValidationAsync(TestModel root, ValidationContext context)
        {
            ValidateBeforeValidationCount++;
            return Task.CompletedTask;
        }

        public override IAsyncEnumerable<ValidationFailure> ValidateAsync(TestModel child, ValidationContext context)
        {
            ValidateCount++;

            return null;
        }

        public override Task<bool> CanValidateAsync(TestModel child, ValidationContext context)
        {
            CanValidateCount++;
            return Task.FromResult(true);
        }

        public override string[] RuleSets => new string[] { };
    }
}
