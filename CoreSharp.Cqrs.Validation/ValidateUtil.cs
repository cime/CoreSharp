using System;
using System.Threading.Tasks;
using CoreSharp.Validation;
using CoreSharp.Validation.Abstractions;
using FluentValidation;

namespace CoreSharp.Cqrs.Validation
{
    public static class ValidateUtil
    {

        public static void Validate(Action validationAction)
        {

            try
            {
                validationAction.Invoke();
            } catch(ValidationException ve)
            {
                var rsp = ve.ToValidationErrorResponse();
                throw new ValidationErrorResponseException(rsp);

            }

        }

        public static async Task ValidateAsync(Func<Task> validationAction)
        {

            try
            {
                await validationAction.Invoke();
            }
            catch (ValidationException ve)
            {
                var rsp = ve.ToValidationErrorResponse();
                throw new ValidationErrorResponseException(rsp);

            }

        }

    }
}
