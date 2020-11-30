using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Grpc.Processors;
using CoreSharp.Cqrs.Grpc.Server;

namespace SimpleInjector
{
    public static class GrpcServerContainerExtensions
    {
        public static Container RegisterCqrsGrpcServers(this Container container, IEnumerable<GrpcCqrsServerConfiguration> configurations)
        {
            container.Register<IGrpcCqrsServerProcessor, GrpcCqrsServerProcessor>();
            container.Collection.Register<GrpcCqrsServer>(configurations.Select(x => Lifestyle.Singleton.CreateRegistration(
                () => new GrpcCqrsServer(x, container), container
            )).ToArray());
            return container;
        }
    }
}
