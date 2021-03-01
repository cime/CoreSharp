using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation.Tests.Models
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
        public override IEnumerable<ValidationFailure> Validate(TRoot child, ValidationContext context)
        {
            if (child.Name == "Invalid")
            {
                yield return Failure("Invalid name", context);
            }

            yield return string.IsNullOrEmpty(child.Name) ? Failure(o => o.Name, "Should not be empty", context) : Success;
        }

        public override bool CanValidate(TRoot child, ValidationContext context)
        {
            return true;
        }

        public override string[] RuleSets => new string[] { };
    }

}
