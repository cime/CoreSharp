using System;
using System.Linq;
using CoreSharp.Cqrs.Grpc.Aspects;
using CoreSharp.Cqrs.Grpc.Common;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class GrpcClientAuthorizationForwardAspect : IGrpcClientAspect
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public GrpcClientAuthorizationForwardAspect(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AfterExecution(object rsp, Exception e)
        {
        }

        public void BeforeExecution(object req)
        {
        }

        public void OnCall(CallOptions callOptions, object channelRequest, GrpcCqrsCallOptions cqrsCallOptions)
        {
            // auth header passthrough
            if ((cqrsCallOptions == null || !cqrsCallOptions.AddInternalAuthorization)
                && _httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue("authorization", out StringValues vs) == true
                && !string.IsNullOrWhiteSpace(vs.ToString()))
            {
                var key = "authorization";

                // remove existing header
                var existingAuth = callOptions.Headers.FirstOrDefault(x => x.Key == key);
                if (existingAuth != null)
                {
                    callOptions.Headers.Remove(existingAuth);
                }

                // add new header
                // note: avoid very long headers, it will result in an error
                var authHeader = vs.ToString();
                callOptions.Headers.Add(key, authHeader);
            }
        }

    }
}
