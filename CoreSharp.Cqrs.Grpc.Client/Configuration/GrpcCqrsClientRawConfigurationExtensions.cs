using CoreSharp.Common.Extensions;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public static class GrpcCqrsClientRawConfigurationExtensions
    {
        public static GrpcCqrsClientConfiguration ToConfiguration(this GrpcCqrsClientRawConfiguration raw)
        {

            var cfg = new GrpcCqrsClientConfiguration
            {
                Url = raw.Url,
                HandleExceptions = raw.HandleExceptions,
                Port = raw.Port,
                TimeoutMs = raw.TimeoutMs,
                ServiceNamePrefix = raw.ServiceNamePrefix,
                ContractsAssemblies = raw.ContractsAssemblies.ToAssemblies(),
                ClientId = raw.ClientId
            };
            return cfg;
        }
    }
}
