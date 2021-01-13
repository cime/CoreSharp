using System;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class GrpcClientExecutionException : Exception
    {
        public GrpcClientExecutionException(string message, Exception innerException = null) : base(message, innerException)
        {
        }
  
    }
}
