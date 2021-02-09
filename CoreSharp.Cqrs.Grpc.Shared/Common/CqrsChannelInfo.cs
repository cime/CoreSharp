using System;
using System.Collections.Generic;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Common
{
    public class CqrsChannelInfo : CqrsInfo
    {
        public Type ChReqType { get; }

        public Type ChRspType { get; }

        public Type ChRspEnvType { get; }

        internal CqrsChannelInfo(Type reqType, string serviceName, string methodName, string formatter, bool isQuery, bool isCommand, bool isAsync, bool isAuthorize, Type rspType, Type chReqType, Type chRspType, Type chRspEnvType, IEnumerable<string> permissions) 
            : base(reqType, serviceName, methodName, formatter, isQuery, isCommand, isAsync, isAuthorize, rspType, permissions)
        {
            ChReqType = chReqType;
            ChRspType = chRspType;
            ChRspEnvType = chRspEnvType;
        }
    }
}
