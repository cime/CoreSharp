using System;
using System.CodeDom;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using CoreSharp.Validation;

namespace CoreSharp.Tests.Validation.Models
{
    public class GenericRootModel
    {
        public string Name { get; set; }

        public List<GenericRootModel> Children { get; set; } = new List<GenericRootModel>();

        public GenericRootModel Relation { get; set; }
    }


    public class GenericRootModelValidator : Validator<GenericRootModel>
    {
        public GenericRootModelValidator()
        {
            RuleForEach(o => o.Children).SetValidator(this);
            RuleFor(o => o.Relation).SetValidator(this);
        }
    }

    public class GenericRootDomainValidator<TRoot> : AbstractDomainValidator<TRoot>
        where TRoot : GenericRootModel
    {
        public override ValidationFailure Validate(TRoot child, ValidationContext context)
        {
            return string.IsNullOrEmpty(child.Name) ? Failure("Name should not be empty", context) : Success;
        }

        public override bool CanValidate(TRoot child, ValidationContext context)
        {
            return true;
        }

        public override string[] RuleSets => new string[] { };
    }

}
