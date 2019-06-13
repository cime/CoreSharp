using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public interface IDomainValidator
    {
        ValidationFailure Validate(object model, ValidationContext context);

        bool CanValidate(object model, ValidationContext context);

        string[] RuleSets { get; }
    }

    public interface IDomainValidator<TModel> : IDomainValidator
    {
        ValidationFailure Validate(TModel model, ValidationContext context);

        bool CanValidate(TModel model, ValidationContext context);
    }
}
