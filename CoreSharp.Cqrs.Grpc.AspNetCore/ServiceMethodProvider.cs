using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Grpc.Contracts;
using CoreSharp.Cqrs.Grpc.Mapping;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using SimpleInjector;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class ServiceMethodProvider : IServiceMethodProvider<GrpcService>
    {
        private readonly GrpcCqrsAspNetCoreConfiguration _cfg;
        private readonly CqrsContractsAdapter _cqrs;
        private readonly Container _container;

        public ServiceMethodProvider(CqrsContractsAdapter cqrs, GrpcCqrsAspNetCoreConfiguration cfg, Container container)
        {
            _cfg = cfg;
            _cqrs = cqrs;
            _container = container;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<GrpcService> context)
        {

            // mapper
            var mapperValidator = _cfg.MapperValidator != null ? Activator.CreateInstance(_cfg.MapperValidator) as IPropertyMapValidator : null;
            var mapper = _cqrs.CreateMapper(mapperValidator);

            // set server id for logging
            var serverId = !string.IsNullOrWhiteSpace(_cfg.ServerId) ? _cfg.ServerId : Assembly.GetEntryAssembly().FullName.Split(',')[0];

            // create methos
            var methodAddGeneric = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == nameof(AddMethod));
            _cqrs.ToCqrsChannelInfo().ToList().ForEach(x => {
                var method = GrpcMethodFactoryUtil.CreateGrpcMethodGeneric(x);
                var invokeMethod = GrpcServerMethodFactoryUtil.CreateServerMethodGeneric(_container, serverId, x, mapper, _cfg.TimeoutMs);
                var methodAdd = methodAddGeneric.MakeGenericMethod(x.ChReqType, x.ChRspEnvType);
                methodAdd.Invoke(this, new object[] { context, method, invokeMethod });
            });
        }

        private void AddMethod<TChReq,TchRsp>(ServiceMethodProviderContext<GrpcService> context, Method<TChReq, TchRsp> method, UnaryServerMethod<GrpcService, TChReq, TchRsp> unaryServerMethod)
            where TChReq : class
            where TchRsp : class
        {
            context.AddUnaryMethod(method, new List<object>(), unaryServerMethod);
        }

    }
}
