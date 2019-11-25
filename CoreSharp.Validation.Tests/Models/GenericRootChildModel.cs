using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using CoreSharp.Validation;

namespace CoreSharp.Tests.Validation.Models
{
    public class GenericRootChildParent
    {
        public List<GenericRootChildChild> Children { get; set; } = new List<GenericRootChildChild>();

        public GenericRootChildChild Relation { get; set; }
    }

    public abstract class GenericRootChildChild
    {
        public string Name { get; set; }
    }

    public class ConcreteGenericRootChildChild : GenericRootChildChild
    {
    }

    public class ConcreteGenericRootChildChild2 : GenericRootChildChild
    {
    }

    public class GenericRootChildParentValidator : Validator<GenericRootChildParent>
    {
        public GenericRootChildParentValidator(IValidator<GenericRootChildChild> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
            RuleFor(o => o.Relation).SetValidator(childValidator);
        }
    }

    public class GenericRootChildDomainValidator<TRoot, TChild> : AbstractDomainValidator<TRoot, TChild>
        where TRoot : GenericRootChildParent
        where TChild : GenericRootChildChild
    {
        public override ValidationFailure Validate(TChild child, ValidationContext context)
        {
            if (child.Name == "Invalid")
            {
                return Failure("Invalid name", context);
            }

            return string.IsNullOrEmpty(child.Name) ? Failure(o => o.Name, "Should not be empty", context) :  Success;
        }

        public override bool CanValidate(TChild child, ValidationContext context)
        {
            return true;
        }

        public override string[] RuleSets => new string[] {};
    }

}
