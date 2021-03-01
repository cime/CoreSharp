using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation.Tests.Models
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
        public override IEnumerable<ValidationFailure> Validate(TChild child, ValidationContext context)
        {
            if (child.Name == "Invalid")
            {
                yield return Failure("Invalid name", context);
            }

            yield return string.IsNullOrEmpty(child.Name) ? Failure(o => o.Name, "Should not be empty", context) : Success;
        }

        public override bool CanValidate(TChild child, ValidationContext context)
        {
            return true;
        }
    }

}
