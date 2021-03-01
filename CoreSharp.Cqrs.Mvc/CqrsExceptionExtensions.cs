using System;
using CoreSharp.Validation;
using CoreSharp.Validation.Abstractions;

namespace CoreSharp.Cqrs.Mvc
{
    internal static class CqrsExceptionExtensions
    {

        public static ValidationErrorResponse ToValidationErrorResponse(this Exception e)
        {
            // get validation errorr response
            if(e is ValidationErrorResponseException)
            {
                return (e as ValidationErrorResponseException)?.Response;
            }

            // throw if not available
            throw e;
        }

    }
}
