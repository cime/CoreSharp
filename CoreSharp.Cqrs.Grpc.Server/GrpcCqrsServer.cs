using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Grpc.Contracts;
using CoreSharp.Cqrs.Grpc.Mapping;
using CoreSharp.Cqrs.Grpc.Processors;
using CoreSharp.Cqrs.Resolver;
using Grpc.Core;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using GrpcServer = Grpc.Core.Server;
namespace CoreSharp.Cqrs.Grpc.Server
{
    public class GrpcCqrsServer
    {
        private readonly Container _container;

        private readonly GrpcCqrsServerConfiguration _configuration;

        private readonly IEnumerable<CqrsChannelInfo> _cqrsChannelInfos;

        private readonly IMapper _mapper;

        private readonly GrpcServer _server;

        public GrpcCqrsServer(GrpcCqrsServerConfiguration configuration, Container container)
        {
            _container = container;
            _configuration = configuration;

            // resolve cqrs from assemblies
            var cqrs = configuration.ContractsAssemblies.SelectMany(CqrsInfoResolverUtil.GetCqrsDefinitions).ToList();
            if (configuration.RegisteredOnly)
            {
                var allTypes = container.GetCurrentRegistrations().Select(x => x.Registration.ImplementationType).SelectMany(x => x.GetInterfaces());
                cqrs = cqrs.Where(x => allTypes.Contains(x.GetHandlerType())).ToList();
            }
            var cqrsApater = new CqrsContractsAdapter(cqrs, _configuration.ServiceNamePrefix);

            // types map 
            _cqrsChannelInfos = cqrsApater.ToCqrsChannelInfo();

            // set server id for logging
            var serverId = !string.IsNullOrWhiteSpace(configuration.ServerId) ? configuration.ServerId : Assembly.GetEntryAssembly().FullName.Split(',')[0];

            // create server 
            var createServiceMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(x => x.Name == nameof(GrpcCqrsServer.CreateGrpcMethodForCqrsChannel));
            _server = new GrpcServer()
            {
                Ports = { { configuration.Url, configuration.Port, ServerCredentials.Insecure } },
            };
            _cqrsChannelInfos.ForEach(x => {
                var create = createServiceMethod.MakeGenericMethod(x.ReqType, x.RspType, x.ChReqType, x.ChRspType, x.ChRspEnvType);
                create.Invoke(this, new object[] { _server, serverId, x });
            });

            // create mapper 
            var mapperValidator = configuration.MapperValidator != null ? Activator.CreateInstance(configuration.MapperValidator) as IPropertyMapValidator : null;
            _mapper = cqrsApater.CreateMapper(mapperValidator);

        }

        public string Id => $"{_configuration.Url}:{_configuration.Port}";

        internal IEnumerable<Assembly> ContractsAssemblies => _configuration.ContractsAssemblies.ToList();

        public void Start()
        {
            _server.Start();
        }

        public async Task StopAsync()
        {
            await _server.ShutdownAsync();
        }

        private void CreateGrpcMethodForCqrsChannel<TRequest, TResponse, TChRequest, TChResponse, TChResponseEnvelope>(GrpcServer server, string serverId, CqrsChannelInfo info)
            where TRequest : class
            where TResponse : class
            where TChRequest : class
            where TChResponse : class
            where TChResponseEnvelope : class
        {

            var builder = ServerServiceDefinition.CreateBuilder();
            var method = GrpcMethodFactoryUtil.CreateGrpcMethod<TChRequest, TChResponseEnvelope>(info.ServiceName, info.MethodName);
            var handler = new UnaryServerMethod<TChRequest, TChResponseEnvelope>(async (chReq, ctx) => {


                // timeout 
                var cancellationToken = ctx.CancellationToken.AddTimeout(_configuration.TimeoutMs);

                // execute in scope
                using (Scope scope = AsyncScopedLifestyle.BeginScope(_container))
                {

                    // execute method
                    var chRspEnv = await GrpcChannelHandlerUtil.HandleChannelRequest<TRequest, TResponse, TChRequest, TChResponse, TChResponseEnvelope>(
                        scope.Container,
                        _mapper,
                        ctx,
                        serverId,
                        info,
                        chReq,
                        cancellationToken);
                    return chRspEnv;
                }
            });

            // add method to server
            builder.AddMethod(method, handler);
            var service = builder.Build();
            server.Services.Add(service);
        }

    }
}
