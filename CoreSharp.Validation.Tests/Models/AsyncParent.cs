using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation.Tests.Models
{
    public class AsyncParent
    {
        public List<AsyncChild> Children { get; set; } = new List<AsyncChild>();
    }

    public class AsyncParentValidator : Validator<AsyncParent>
    {
        public AsyncParentValidator(IValidator<AsyncChild> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
        }
    }

    public class AsyncChild
    {
        public string Name { get; set; }

        public List<AsyncSubChild> Children { get; set; } = new List<AsyncSubChild>();
    }

    public class AsyncChildValidator : Validator<AsyncChild>
    {
        public AsyncChildValidator(IValidator<AsyncSubChild> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
        }
    }

    public class AsyncSubChild
    {
        [NotNull]
        public string Name { get; set; }
    }

    public class AsyncParentDomainValidator : TestAsyncDomainValidator<AsyncParent, AsyncParent>
    { }

    public class AsyncParentChildDomainValidator : TestAsyncDomainValidator<AsyncParent, AsyncChild>
    { }

    public class AsyncParentChild2DomainValidator : TestAsyncDomainValidator<AsyncParent, AsyncChild, int>
    { }

    public class AsyncParentSubChildDomainValidator : TestAsyncDomainValidator<AsyncParent, AsyncSubChild>
    { }

    public class AsyncChildSubChildDomainValidator : TestAsyncDomainValidator<AsyncChild, AsyncSubChild>
    { }

    public class AsyncSubChildDomainValidator : TestAsyncDomainValidator<AsyncSubChild, AsyncSubChild>
    { }

    public abstract class TestAsyncDomainValidator<TModel, TChild> : TestAsyncDomainValidator<TModel, TChild, byte>
        where TModel : class
        where TChild : class
    { }

    public abstract class TestAsyncDomainValidator<TModel, TChild, TType> : AbstractAsyncDomainValidator<TModel, TChild>
        where TModel : class
        where TChild : class
    {
        public static List<IAsyncDomainValidator> Instances = new List<IAsyncDomainValidator>();
        public static List<Tuple<TChild, IAsyncDomainValidator>> ValidateModels = new List<Tuple<TChild, IAsyncDomainValidator>>();
        public static List<Tuple<TChild, IAsyncDomainValidator>> CanValidateModels = new List<Tuple<TChild, IAsyncDomainValidator>>();
        public static List<Tuple<TModel, IAsyncDomainValidator>> BeforeValidationModels = new List<Tuple<TModel, IAsyncDomainValidator>>();

        public TestAsyncDomainValidator()
        {
            Instances.Add(this);
        }

        public override Task BeforeValidationAsync(TModel root, ValidationContext context)
        {
            BeforeValidationModels.Add(new Tuple<TModel, IAsyncDomainValidator>(root, this));
            return Task.CompletedTask;
        }

        public override async IAsyncEnumerable<ValidationFailure> ValidateAsync(TChild child, ValidationContext context)
        {
            ValidateModels.Add(new Tuple<TChild, IAsyncDomainValidator>(child, this));

            yield break;
        }

        public override Task<bool> CanValidateAsync(TChild child, ValidationContext context)
        {
            CanValidateModels.Add(new Tuple<TChild, IAsyncDomainValidator>(child, this));
            return Task.FromResult(true);
        }

        public override string[] RuleSets => new string[] { };

        public static void Clear()
        {
            Instances.Clear();
            ValidateModels.Clear();
            CanValidateModels.Clear();
            BeforeValidationModels.Clear();
        }
    }
}
