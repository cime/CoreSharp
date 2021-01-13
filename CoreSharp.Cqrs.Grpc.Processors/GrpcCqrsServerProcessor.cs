using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Query;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Processors
{
    public class GrpcCqrsServerProcessor : IGrpcCqrsServerProcessor
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryProcessor _queryProcessor;

        public GrpcCqrsServerProcessor(ICommandDispatcher commandDispatcher, IQueryProcessor queryProcessor)
        {
            _commandDispatcher = commandDispatcher;
            _queryProcessor = queryProcessor;
        }

        public async Task<GrpcResponseEnvelope<TResponse>> ProcessRequestAsync<TRequest, TResponse>(TRequest request, CqrsInfo info, CancellationToken cancellationToken)
        {

            if(info.IsCommand)
            {
                if(info.IsAsync)
                {
                    if(info.RspType != null)
                    {
                        var cmd = request as IAsyncCommand<TResponse>;
                        var rsp = await _commandDispatcher.DispatchAsync(cmd, cancellationToken);
                        return CreateResponseEnvelope(rsp);
                    } else
                    {

                    }
                } else
                {
                    if(info.RspType != null)
                    {
                        var cmd = request as ICommand<TResponse>;
                        var rsp = await Task.Run(() =>
                        {
                            return _commandDispatcher.Dispatch(cmd);
                        }, cancellationToken);
                        return CreateResponseEnvelope(rsp);
                    }
                }

            }

            if(info.IsQuery)
            {
                if(info.IsAsync)
                {
                    var query = request as IAsyncQuery<TResponse>;
                    var rsp = await _queryProcessor.HandleAsync(query, cancellationToken);
                    return CreateResponseEnvelope(rsp);
                } else
                {
                    var query = request as IQuery<TResponse>;
                    var rsp = await Task.Run(() =>
                    {
                        return _queryProcessor.Handle(query);
                    }, cancellationToken);
                    return CreateResponseEnvelope(rsp);
                }
            }

            // not supported
            return CreateResponseEnvelopeError<TResponse>("Not supported request.");
        }

        private GrpcResponseEnvelope<TResponse> CreateResponseEnvelope<TResponse>(TResponse response)
        {
            return new GrpcResponseEnvelope<TResponse>
            {
                Value = response
            };
        }

        private GrpcResponseEnvelope<TResponse> CreateResponseEnvelopeError<TResponse>(string errorMsg = null)
        {
            return new GrpcResponseEnvelope<TResponse>
            {
                IsExecutionError = true,
                ErrorMessage = errorMsg
            };
        }

    }
}
