using System.Linq;
using CoreSharp.Validation;

namespace FluentValidation
{
    public static class ValidationExceptionExtensions
    {
        public static ValidationErrorResponse ToValidationErrorResponse(this ValidationException excetion)
        {
            var rsp = new ValidationErrorResponse
            {
                ErrorMessage = excetion.Message,
                Errors = excetion.Errors.Select(x => new ValidationErrorResponse.ValidationError
                {
                    PropertyName = x.PropertyName,
                    AttemptedValue = x.AttemptedValue,
                    ErrorCode = x.ErrorCode,
                    ErrorMessage = x.ErrorMessage,
                    Severity = (ValidationErrorResponse.ErrorSeverity) (int) x.Severity
                }).ToArray()
            };
            return rsp;
        }
    }
}
