using System;
using CoreSharp.Cqrs.Grpc.Common;
using Grpc.Core;

namespace CoreSharp.Cqrs.Grpc.Aspects
{
    public interface IGrpcClientAspect
    {
        void OnCall(CallOptions callOptions, object channelRequest, GrpcCqrsCallOptions cqrsCallOptions);

        void BeforeExecution(object req);

        void AfterExecution(object rsp, Exception e);
    }
}
