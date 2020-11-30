using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Grpc.Server;
using Microsoft.Extensions.Configuration;

namespace SimpleInjector
{
    public static class GrpcHostContainerExtensions
    {

        public static Container RegisterCqrsGrpcServersHost(this Container container, string cfgKey = "GrpcServers")
        {
            var serversCfg = container.GetRegisteredInstance<IConfiguration>().GetSection(cfgKey).Get<IEnumerable<GrpcCqrsServerRawConfiguration>>()
                .Select(x => x.ToConfiguration()).ToList();
            container.RegisterCqrsGrpcServers(serversCfg);
            container.Register<GrpcCqrsServerHost, GrpcCqrsServerHost>();
            return container;
        }

    }
}
