using CoreSharp.Validation;
using CoreSharp.Validation.Abstractions;

namespace CoreSharp.Cqrs.Grpc.Common
{

    public class GrpcResponseEnvelope
    {
        public bool IsExecutionError { get; set; }

        public bool IsValidationError { get; set; }

        public string ErrorMessage { get; set; }

        public ValidationErrorResponse ValidationError { get; set; }
    }

    public class GrpcResponseEnvelope<T> : GrpcResponseEnvelope
    {
        public T Value { get; set; }
    }
}
