using System.Collections.Generic;
using CoreSharp.Identity.Jwt;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class GrpcCqrsClientRawConfiguration
    {
        public string ServiceNamePrefix { get; set; } = "Cqrs_Grpc_";

        public string Url { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 5500;

        public int TimeoutMs { get; set; } = 10000;

        public bool HandleExceptions { get; set; }

        public bool HandleUnauthenticated { get; set; } = true;

        public IEnumerable<string> ContractsAssemblies { get; set; }

        public string ClientId { get; set; }

        public EnumChannelAuthorizationType AuthorizationType { get; set; }

        public TokenConfiguration TokenConfiguration { get; set; }

        public GrpcCqrsCallOptions DefaultCallOptions { get; set; } = new GrpcCqrsCallOptions
        {
            AddInternalAuthorization = true
        };
    }
}
