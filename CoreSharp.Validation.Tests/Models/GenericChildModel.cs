using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using CoreSharp.Validation;

namespace CoreSharp.Tests.Validation.Models
{
    public class GenericChildParent
    {
        public List<GenericChildChild> Children { get; set; } = new List<GenericChildChild>();

        public GenericChildChild Relation { get; set; }
    }


    public abstract class GenericChildChild
    {
        public string Name { get; set; }
    }

    public class ConcreteGenericChildChild : GenericChildChild
    {
    }

    public class ConcreteGenericChildChild2 : GenericChildChild
    {
    }

    public class GenericChildParentValidator : Validator<GenericChildParent>
    {
        public GenericChildParentValidator(IValidator<GenericChildChild> childValidator)
        {
            RuleForEach(o => o.Children).SetValidator(childValidator);
            RuleFor(o => o.Relation).SetValidator(childValidator);
        }
    }

    public class GenericChildDomainValidator<TChild> : AbstractDomainValidator<GenericChildParent, TChild> where TChild : GenericChildChild
    {
        public override ValidationFailure Validate(TChild child, ValidationContext context)
        {
            return string.IsNullOrEmpty(child.Name) ? Failure("Name should not be empty", context) :  Success;
        }

        public override bool CanValidate(TChild child, ValidationContext context)
        {
            return true;
        }
    }

}
