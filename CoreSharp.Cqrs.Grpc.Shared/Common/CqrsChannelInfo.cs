using System;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Common
{
    public class CqrsChannelInfo : CqrsInfo
    {
        public Type ChReqType { get; }

        public Type ChRspType { get; }

        public Type ChRspEnvType { get; }

        internal CqrsChannelInfo(Type reqType, string serviceName, string methodName, string formatter, bool isQuery, bool isCommand, bool isAsync, Type rspType, Type chReqType, Type chRspType, Type chRspEnvType) 
            : base(reqType, serviceName, methodName, formatter, isQuery, isCommand, isAsync, rspType)
        {
            ChReqType = chReqType;
            ChRspType = chRspType;
            ChRspEnvType = chRspEnvType;
        }
    }
}
