using System;

namespace CoreSharp.Validation.Abstractions
{
    public class ValidationErrorResponseException : Exception
    {
        public ValidationErrorResponseException(ValidationErrorResponse errorResponse) : base(errorResponse.ErrorMessage)
        {
            Response = errorResponse;
        }

        public ValidationErrorResponse Response { get; }
    }
}
