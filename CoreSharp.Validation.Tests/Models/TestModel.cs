using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using CoreSharp.Validation;

namespace CoreSharp.Tests.Validation.Models
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
        public static int ValidatBeforeValidationCount;

        public override void BeforeValidation(TestModel root, ValidationContext context)
        {
            ValidatBeforeValidationCount++;
        }

        public override ValidationFailure Validate(TestModel child, ValidationContext context)
        {
            ValidateCount++;
            return Success;
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
        public static int ValidatBeforeValidationCount;

        public override Task BeforeValidationAsync(TestModel root, ValidationContext context)
        {
            ValidatBeforeValidationCount++;
            return Task.CompletedTask;
        }

        public override Task<ValidationFailure> ValidateAsync(TestModel child, ValidationContext context)
        {
            ValidateCount++;
            return Task.FromResult(Success);
        }

        public override Task<bool> CanValidateAsync(TestModel child, ValidationContext context)
        {
            CanValidateCount++;
            return Task.FromResult(true);
        }

        public override string[] RuleSets => new string[] { };
    }
}
