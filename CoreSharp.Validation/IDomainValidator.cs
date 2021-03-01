using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public interface IDomainValidator
    {
        void BeforeValidation(object model, ValidationContext context);

        IEnumerable<ValidationFailure> Validate(object model, ValidationContext context);

        bool CanValidate(object model, ValidationContext context);

        string[] RuleSets { get; }
    }

    public interface IDomainValidator<TModel> : IDomainValidator<TModel, TModel>
    {
    }

    public interface IDomainValidator<TRoot, TChild> : IDomainValidator
    {
        void BeforeValidation(TRoot root, ValidationContext context);

        IEnumerable<ValidationFailure> Validate(TChild child, ValidationContext context);

        bool CanValidate(TChild child, ValidationContext context);
    }
}
