using System.Collections.Generic;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class GrpcCqrsAspNetCoreRawConfiguration
    {
        public string ServiceNamePrefix { get; set; } = "Cqrs_Grpc_";

        public int TimeoutMs { get; set; } = 10000;

        public IEnumerable<string> ContractsAssemblies { get; set; }

        public string ServerId { get; set; }

        public bool ExposeProto { get; set; } = true;

        public string MapperValidator { get; set; }
    }
}
