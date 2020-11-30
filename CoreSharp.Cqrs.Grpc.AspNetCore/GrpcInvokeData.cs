using System;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class GrpcInvokeData
    {

        public GrpcInvokeData(object method, object invokeMethod, Type chReqType, Type chRspEnvType)
        {
            ChReqType = chReqType;
            ChRspEnvType = chRspEnvType;
            Method = method;
            IvokeMethod = invokeMethod;
        }

        public Type ChReqType { get; }

        public Type ChRspEnvType { get; }

        public object Method { get; }

        public object IvokeMethod { get; }
    }
}
