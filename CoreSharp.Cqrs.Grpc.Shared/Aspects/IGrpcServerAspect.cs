using System;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Grpc.Common;
using Grpc.Core;
using SimpleInjector;

namespace CoreSharp.Cqrs.Grpc.Aspects
{
    public interface IGrpcServerAspect
    {
        void OnCallRecieved(ServerCallContext callContext);

        void BeforeExecution(object req);

        void AfterExecution(object rsp, Exception e);

        Task<GrpcResponseEnvelope<TRsp>> ExecuteAsync<TRsp>(Container container, Func<Task<GrpcResponseEnvelope<TRsp>>> next);
    }
}
