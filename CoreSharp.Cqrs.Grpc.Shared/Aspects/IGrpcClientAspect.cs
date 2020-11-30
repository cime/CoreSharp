using System;
using Grpc.Core;

namespace CoreSharp.Cqrs.Grpc.Aspects
{
    public interface IGrpcClientAspect
    {
        void OnCall(CallOptions callOptions, object channelRequest);

        void BeforeExecution(object req);

        void AfterExecution(object rsp, Exception e);
    }
}
