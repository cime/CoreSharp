using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public interface IAsyncDomainValidator
    {
        Task BeforeValidationAsync(object model, ValidationContext context);

        IAsyncEnumerable<ValidationFailure> ValidateAsync(object model, ValidationContext context);

        Task<bool> CanValidateAsync(object model, ValidationContext context);

        string[] RuleSets { get; }
    }

    public interface IAsyncDomainValidator<TModel> : IAsyncDomainValidator<TModel, TModel>
    {
    }

    public interface IAsyncDomainValidator<TRoot, TChild> : IAsyncDomainValidator
    {
        Task BeforeValidationAsync(TRoot root, ValidationContext context);

        IAsyncEnumerable<ValidationFailure> ValidateAsync(TChild child, ValidationContext context);

        Task<bool> CanValidateAsync(TChild child, ValidationContext context);
    }
}
