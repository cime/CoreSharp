using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Aspects;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Resolver;
using CoreSharp.Validation;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace CoreSharp.Cqrs.Grpc.Processors
{
    public static class GrpcChannelHandlerUtil
    {

        public static async Task<TChRspEnvelope> HandleChannelRequest<TReq, TRsp, TChReq, TChRsp, TChRspEnvelope>(
            Container container, 
            IMapper mapper,
            ServerCallContext ctx,
            string serverId,
            CqrsInfo info,
            TChReq chReq,
            CancellationToken cancellationToken = default)
        {

            GrpcResponseEnvelope<TRsp> rsp = null;
            Exception exception = null;

            // get logger
            var logger = (container.TryGetInstance(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger("CqrsGrpcServer");

            // get aspect
            var aspect = container.TryGetInstance(typeof(IGrpcServerAspect)) as IGrpcServerAspect;

            // call recieved
            aspect?.OnCallRecieved(ctx);

            // name
            var name = chReq?.GetType()?.Name ?? "Unknown";

            var watch = Stopwatch.StartNew();
            try
            {

                // before execution
                logger?.LogDebug("Request {requestName} started on server {serverId}.", name, serverId);
                var req = mapper.Map<TReq>(chReq);
                aspect?.BeforeExecution(req);

                // execution function
                Func<Task<GrpcResponseEnvelope<TRsp>>> execFnc = () =>
                {
                    var processor = container.GetInstance<IGrpcCqrsServerProcessor>();
                    return Task.Run(async () => await processor.ProcessRequestAsync<TReq, TRsp>(req, info, cancellationToken));
                };

                // execute
                rsp = await (aspect != null ? aspect.ExecuteAsync(container, execFnc) : execFnc.Invoke());

                // execution completed
                var chRsp = mapper.Map<TChRspEnvelope>(rsp);
                watch.Stop();
                logger?.LogInformation("Request {requestName} completed on server {serverId}. Call duration was {durationMs}ms.", name, serverId, watch.ElapsedMilliseconds);
                return chRsp;

            }
            catch(ValidationErrorResponseException e)
            {
                exception = e;
                watch.Stop();
                logger.LogInformation("Request {requestName} validation error on server {serverId}. Call duration was {durationMs}ms.", name, serverId, watch.ElapsedMilliseconds);

                rsp = new GrpcResponseEnvelope<TRsp>
                {
                    IsValidationError = true,
                    ValidationError = e.Response
                };
                var chRsp = mapper.Map<TChRspEnvelope>(rsp);
                return chRsp;
            }
            catch (Exception e)
            {
                exception = e;
                watch.Stop();
                logger.LogError(e, "Request {requestName} failed on server {serverId}. Call duration was {durationMs}ms.", name, serverId, watch.ElapsedMilliseconds);

                rsp = new GrpcResponseEnvelope<TRsp>
                {
                    IsExecutionError = true,
                    ErrorMessage = e.Message
                };
                var chRsp = mapper.Map<TChRspEnvelope>(rsp);
                return chRsp;

            }
            finally
            {
                aspect?.AfterExecution(rsp, exception);
            }

        }
    }
}
