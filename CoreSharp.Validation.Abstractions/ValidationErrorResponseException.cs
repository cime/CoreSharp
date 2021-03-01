using System;

namespace CoreSharp.Validation
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
