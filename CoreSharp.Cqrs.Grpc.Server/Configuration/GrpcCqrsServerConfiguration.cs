using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.Cqrs.Grpc.Server
{

    public class GrpcCqrsServerConfiguration
    {
        public string ServiceNamePrefix { get; set; } = "Cqrs_Grpc_";

        public string Url { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 5500;

        public int TimeoutMs { get; set; } = 10000;

        public bool RegisteredOnly { get; set; } = true;

        public IEnumerable<Assembly> ContractsAssemblies { get; set; }

        public Type MapperValidator { get; set; }

        public string ServerId { get; set; }
    }

}
