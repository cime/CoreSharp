using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class GrpcCqrsAspNetCoreConfiguration
    {
        public bool ExposeProto { get; set; } = true;

        public string ServiceNamePrefix { get; set; } = "Cqrs_Grpc_";

        public int TimeoutMs { get; set; } = 10000;

        public IEnumerable<Assembly> ContractsAssemblies { get; set; }

        public Type MapperValidator { get; set; }

        public string ServerId { get; set; }
    }
}
