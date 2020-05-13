using System;
using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation.Tests.Models
{
    public class Parent
    {
        public List<Child> Children { get; set; } = new List<Child>();
    }

    public class ParentValidator : Validator<Parent>
    {
        public ParentValidator(IValidator<Child> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
        }
    }

    public class Child
    {
        public string Name { get; set; }

        public List<SubChild> Children { get; set; } = new List<SubChild>();
    }

    public class ChildValidator : Validator<Child>
    {
        public ChildValidator(IValidator<SubChild> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
        }
    }

    public class SubChild
    {
        [NotNull]
        public string Name { get; set; }
    }

    public class ParentDomainValidator : TestDomainValidator<Parent, Parent>
    { }

    public class ParentChildDomainValidator : TestDomainValidator<Parent, Child>
    { }

    public class ParentChild2DomainValidator : TestDomainValidator<Parent, Child, int>
    { }

    public class ParentSubChildDomainValidator : TestDomainValidator<Parent, SubChild>
    { }

    public class ChildSubChildDomainValidator : TestDomainValidator<Child, SubChild>
    { }

    public class SubChildDomainValidator : TestDomainValidator<SubChild, SubChild>
    { }

    public abstract class TestDomainValidator<TModel, TChild> : TestDomainValidator<TModel, TChild, byte>
        where TModel : class
        where TChild : class
    { }

    public abstract class TestDomainValidator<TModel, TChild, TType> : AbstractDomainValidator<TModel, TChild> 
        where TModel : class
        where TChild : class
    {
        public static List<IDomainValidator> Instances = new List<IDomainValidator>();
        public static List<Tuple<TChild, IDomainValidator>> ValidateModels = new List<Tuple<TChild, IDomainValidator>>();
        public static List<Tuple<TChild, IDomainValidator>> CanValidateModels = new List<Tuple<TChild, IDomainValidator>>();
        public static List<Tuple<TModel, IDomainValidator>> BeforeValidationModels = new List<Tuple<TModel, IDomainValidator>>();

        public TestDomainValidator()
        {
            Instances.Add(this);
        }

        public override void BeforeValidation(TModel root, ValidationContext context)
        {
            BeforeValidationModels.Add(new Tuple<TModel, IDomainValidator>(root, this));
        }

        public override ValidationFailure Validate(TChild child, ValidationContext context)
        {
            ValidateModels.Add(new Tuple<TChild, IDomainValidator>(child, this));
            return Success;
        }

        public override bool CanValidate(TChild child, ValidationContext context)
        {
            CanValidateModels.Add(new Tuple<TChild, IDomainValidator>(child, this));
            return true;
        }

        public override string[] RuleSets => new string[] {};

        public static void Clear()
        {
            Instances.Clear();
            ValidateModels.Clear();
            CanValidateModels.Clear();
            BeforeValidationModels.Clear();
        }
    }
}
