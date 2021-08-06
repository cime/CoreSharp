using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Grpc.Aspects;
using CoreSharp.Cqrs.Grpc.Client;
using CoreSharp.Cqrs.Query;
using Microsoft.Extensions.Logging;

namespace SimpleInjector
{

    public static class GrpcClientContainerExtensions
    {

        public static Container RegisterCqrsGrpcClients(this Container container, IEnumerable<GrpcCqrsClientConfiguration> configurations)
        {
            // cqrs grpc 
            container.Register<IGrpcQueryProcessor, GrpcClientQueryProcessor>();
            container.Register<IGrpcCommandDispatcher, GrpcClientCommandDispatcher>();

            // cqrs decorators (route requests to grpc or locally)
            container.RegisterDecorator(typeof(ICommandDispatcher), typeof(CommandDispatcherRouteDecorator), Lifestyle.Transient);
            container.RegisterDecorator(typeof(IQueryProcessor), typeof(QueryProcessorRouteDecorator), Lifestyle.Transient);

            // client 
            container.Collection.Register<GrpcCqrsClient>(configurations.Select(x => Lifestyle.Singleton.CreateRegistration(
                () =>{
                    var clientAspectList = container.TryGetAllInstances(typeof(IGrpcClientAspect))
                        ?.Where(a => a is IGrpcClientAspect)
                        .Select(a => a as IGrpcClientAspect)?.ToList() 
                        ?? new List<IGrpcClientAspect>();
                    var clientAspect = container.TryGetInstance(typeof(IGrpcClientAspect)) as IGrpcClientAspect;
                    if(clientAspect != null)
                    {
                        clientAspectList.Add(clientAspect);
                    }
                    var logger = container.TryGetInstance(typeof(ILogger<GrpcCqrsClient>)) as ILogger<GrpcCqrsClient>;
                    return new GrpcCqrsClient(x, logger, clientAspectList);
                }, container
            )).ToArray());
            container.Register<IGrpcClientManager, GrpcClientManager>();

            return container;
        }

    }
}
