using System.Linq;
using System.Reflection;
using System.Threading;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Grpc.Processors;
using Grpc.AspNetCore.Server.Model;
using SimpleInjector;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    internal static class GrpcServerMethodFactoryUtil
    {

        public static UnaryServerMethod<GrpcService, TChReq, TChRspEnvelope> CreateServerMethod<TReq, TRsp, TChReq, TChRsp, TChRspEnvelope>(
            Container container,
            string serverId,
            CqrsChannelInfo info, 
            IMapper mapper, 
            int timeoutMs
            )
        {
            var method = new UnaryServerMethod<GrpcService, TChReq, TChRspEnvelope>(async (svc, chReq, ctx) => {

                // add timeout to cancellation token
                var cancellationToken = ctx.CancellationToken.AddTimeout(timeoutMs);

                // handle request
                var rsp = await GrpcChannelHandlerUtil.HandleChannelRequest<TReq, TRsp, TChReq, TChRsp, TChRspEnvelope>(
                    container, mapper, ctx, serverId, info, chReq, cancellationToken);
                return rsp;

            });
            return method;
        }

        public static object CreateServerMethodGeneric(
            Container container,
            string serverId,
            CqrsChannelInfo info, 
            IMapper mapper, 
            int timeoutMs)
        {
            var method = typeof(GrpcServerMethodFactoryUtil).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(CreateServerMethod)).MakeGenericMethod(info.ReqType, info.RspType, info.ChReqType, info.ChRspType, info.ChRspEnvType);
            var grpcMethod = method.Invoke(null, new object[] { container, serverId, info, mapper, timeoutMs });
            return grpcMethod;
        }
    }
}
