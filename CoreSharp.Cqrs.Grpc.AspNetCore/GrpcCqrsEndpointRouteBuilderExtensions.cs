using System.Linq;
using System.Net;
using System.Text.Json;
using CoreSharp.Cqrs.Grpc.AspNetCore;
using CoreSharp.Cqrs.Grpc.Contracts;
using Microsoft.AspNetCore.Builder;
using SimpleInjector;

namespace Microsoft.AspNetCore.Routing
{
    public static class GrpcCqrsEndpointRouteBuilderExtensions
    {

        public static GrpcServiceEndpointConventionBuilder MapCqrsGrpc(this IEndpointRouteBuilder endpoints)
        {
            // map proto list 
            endpoints.MapGet("grpc/contracts", async ctx => {

                // get cfg
                Container container = ctx.RequestServices.GetService(typeof(Container)) as Container;
                var cfg = container.GetInstance<GrpcCqrsAspNetCoreConfiguration>();
                if(!cfg.ExposeProto)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // write response
                var payload = JsonSerializer.Serialize(cfg.ContractsAssemblies.Select(x => x.FullName.Split(',')[0]));
                ctx.Response.StatusCode = (int) HttpStatusCode.OK;
                ctx.Response.Headers.Add("Content-Type", "application/json");
                var buffer = System.Text.Encoding.UTF8.GetBytes(payload);
                await ctx.Response.Body.WriteAsync(buffer);

            });

            // map proto file 
            endpoints.MapGet("grpc/contracts/{name}", async ctx => {

                // get cfg
                Container container = ctx.RequestServices.GetService(typeof(Container)) as Container;
                var cfg = container.GetInstance<GrpcCqrsAspNetCoreConfiguration>();
                if (!cfg.ExposeProto)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // check name
                if(!ctx.Request.RouteValues.TryGetValue("name", out object nameObj) 
                    || nameObj as string == null 
                    || string.IsNullOrWhiteSpace(nameObj as string)
                    || !cfg.ContractsAssemblies.Select(x => x.FullName.Split(',')[0]).Any(x => x == nameObj as string))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // get contracts assembly
                var assembly = cfg.ContractsAssemblies.First(x => x.FullName.Split(',')[0] == (nameObj as string));

                // get proto
                var proto = container.GetInstance<CqrsContractsAdapter>().GetProto(assembly);

                // write response
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.Headers.Add("Content-Type", "text/plain");
                var buffer = System.Text.Encoding.UTF8.GetBytes(proto);
                await ctx.Response.Body.WriteAsync(buffer);

            });

            // map grpc
            return endpoints.MapGrpcService<GrpcService>();
        }
    }
}
