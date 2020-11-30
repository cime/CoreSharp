using System;

namespace CoreSharp.Cqrs.Resolver
{
    public class CqrsInfo
    {

        public CqrsInfo(Type reqType, string serviceName, string methodName, string formatter, bool isQuery, bool isCommand, bool isAsync, Type rspType)
        {
            ReqType = reqType;
            RspType = rspType;
            IsAsync = isAsync;
            IsQuery = isQuery;
            IsCommand = isCommand;
            ServiceName = serviceName;
            MethodName = methodName;
            Formatter = formatter;
        }

        public Type ReqType { get; }

        public Type RspType { get; }

        public bool IsAsync { get; }

        public bool IsQuery { get; }

        public bool IsCommand { get; }

        public string ServiceName { get; }

        public string MethodName { get; }

        public string Formatter { get; }
    }
}
