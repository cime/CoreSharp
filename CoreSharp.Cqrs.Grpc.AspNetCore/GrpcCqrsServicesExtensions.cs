using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Grpc.AspNetCore;
using CoreSharp.Cqrs.Grpc.Contracts;
using CoreSharp.Cqrs.Grpc.Processors;
using CoreSharp.Cqrs.Resolver;
using Grpc.AspNetCore.Server.Model;
using SimpleInjector;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GrpcCqrsServicesExtensions
    {

        public static IServiceCollection AddCqrsGrpc(this IServiceCollection services, 
            GrpcCqrsAspNetCoreConfiguration configuration)
        {

            // resolve cqrs
            IEnumerable<CqrsInfo> cqrs = configuration.ContractsAssemblies.SelectMany(CqrsInfoResolverUtil.GetCqrsDefinitions).ToList();
            var cqrsAdapter = new CqrsContractsAdapter(cqrs, configuration.ServiceNamePrefix);

            // register data to container 
            var container = services.GetContainer();
            container.RegisterInstance(cqrsAdapter);
            container.RegisterInstance(configuration);
            container.Register<IGrpcCqrsServerProcessor, GrpcCqrsServerProcessor>();
            container.Register<IServiceMethodProvider<GrpcService>, ServiceMethodProvider>(Lifestyle.Singleton);

            // register grpc
            services.AddGrpc();
            services.AddSingleton(svc => container.GetInstance<IServiceMethodProvider<GrpcService>>());

            // return
            return services;
        }

        private static Container GetContainer(this IServiceCollection services)
        {
            var enumerator = services.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var inst = enumerator.Current?.ImplementationInstance;
                if(inst != null && inst.GetType() == typeof(Container))
                {
                    return inst as Container;
                }
            }
            return null;
        }

    }
}
