using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Resolver;
using CoreSharp.Identity.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public static class GrpcServiceEndpointConventionBuilderExtensions
    {

        public static GrpcServiceEndpointConventionBuilder AddCqrsAuthorization(this GrpcServiceEndpointConventionBuilder builder, IEnumerable<CqrsChannelInfo> cqrs, ILogger logger = null)
        {

            builder.Add(cnv => {

                var execDelegate = cnv.RequestDelegate;
                cnv.RequestDelegate = ctx => {

                    // no path set, this should not happen
                    if(string.IsNullOrWhiteSpace(ctx.Request.Path))
                    {
                        logger?.LogWarning("Request path is not set.");
                        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    }

                    // get path definition
                    var path = ctx.Request.Path.Value.ToLowerInvariant();
                    var definition = cqrs.Where(x => ((CqrsInfo)x).GetPath().ToLowerInvariant().StartsWith(path))
                        .OrderByDescending(x => ((CqrsInfo)x).GetPath().Length).FirstOrDefault();
                    
                    // definition not found, this should not be the case 
                    if(definition == null)
                    {
                        logger?.LogWarning("No CQRS found for request path.");
                        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    }

                    // log required permissions
                    logger?.LogDebug("Authorization required: permissions={permissions}",
                        string.Join(',', definition.Permissions?.ToList() ?? new List<string>()));

                    // authorization required
                    if (definition.IsAuthorize) {

                        // user required for authorization
                        var identity = ctx.User?.Identity as ClaimsIdentity;
                        if (identity == null || !identity.IsAuthenticated)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return Task.CompletedTask;
                        }

                        // log required permissions
                        logger?.LogDebug("Authorization user: permissions={permissions}, system={systemUser}",
                            string.Join(',', identity.GetPermissions()?.ToList() ?? new List<string>()),
                            identity.IsSystemUser());

                        // permissions check
                        if (definition.Permissions != null && definition.Permissions.Any() && !identity.IsSystemUser()
                            && !identity.HasAnyPermission(definition.Permissions.ToArray()))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return Task.CompletedTask;
                        }

                    }

                    // log access
                    logger?.LogDebug("Authorization passed for path {path}.", ctx.Request.Path);

                    // execute action
                    return execDelegate(ctx);
                };

            });

            return builder;
        }

 
    }
}
