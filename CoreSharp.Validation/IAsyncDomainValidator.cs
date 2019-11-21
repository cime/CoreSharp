using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CoreSharp.Validation
{
    public interface IAsyncDomainValidator
    {
        Task<ValidationFailure> ValidateAsync(object model, ValidationContext context);

        Task<bool> CanValidateAsync(object model, ValidationContext context);

        string[] RuleSets { get; }
    }

    public interface IAsyncDomainValidator<TModel> : IAsyncDomainValidator
    {
        Task<ValidationFailure> ValidateAsync(TModel model, ValidationContext context);

        Task<bool> CanValidateAsync(TModel model, ValidationContext context);
    }
}
