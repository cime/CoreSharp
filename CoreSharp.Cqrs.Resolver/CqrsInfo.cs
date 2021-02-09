using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.Resolver
{
    public class CqrsInfo
    {

        public CqrsInfo(Type reqType, string serviceName, string methodName, string formatter, bool isQuery, bool isCommand, bool isAsync, bool isAuthorize, Type rspType, IEnumerable<string> permissions)
        {
            ReqType = reqType;
            RspType = rspType;
            IsAsync = isAsync;
            IsQuery = isQuery;
            IsCommand = isCommand;
            IsAuthorize = isAuthorize;
            ServiceName = serviceName;
            MethodName = methodName;
            Formatter = formatter;
            Permissions = permissions;
        }

        public Type ReqType { get; }

        public Type RspType { get; }

        public bool IsAsync { get; }

        public bool IsQuery { get; }

        public bool IsCommand { get; }

        public bool IsAuthorize { get; }

        public string ServiceName { get; }

        public string MethodName { get; }

        public string Formatter { get; }

        public IEnumerable<string> Permissions { get; }
    }
}
