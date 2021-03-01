using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Aspects;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Grpc.Contracts;
using CoreSharp.Cqrs.Resolver;
using CoreSharp.Validation;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CoreSharp.Cqrs.Grpc.Client
{

    public class GrpcCqrsClient
    {

        private readonly CallInvoker _invoker;

        private readonly IMapper _mapper;

        private readonly Dictionary<Type, object> _grpcMethods;

        private readonly CqrsContractsAdapter _cqrsAdapter;

        private readonly GrpcCqrsClientConfiguration _configuration;

        private ILogger<GrpcCqrsClient> _logger;

        private readonly IGrpcClientAspect _clientAspect;

        public GrpcCqrsClient(GrpcCqrsClientConfiguration configuration, 
            ILogger<GrpcCqrsClient> logger, 
            IGrpcClientAspect clientAspect = null)
        {

            _configuration = configuration;
            _logger = logger;
            _clientAspect = clientAspect;

            // client id definition
            Id = !string.IsNullOrWhiteSpace(_configuration.ClientId) ? _configuration.ClientId : Assembly.GetEntryAssembly().FullName.Split(',')[0];

            // resolve cqrs from assemblies
            var cqrs = configuration.ContractsAssemblies.SelectMany(CqrsInfoResolverUtil.GetCqrsDefinitions).ToList();
            _cqrsAdapter = new CqrsContractsAdapter(cqrs, configuration.ServiceNamePrefix);

            // create grpc invokation methods
            _grpcMethods = _cqrsAdapter.ToCqrsChannelInfo().ToDictionary(
                x => x.ChReqType,
                x => CreateGrpcMethodForCqrsChannel(x));

            // create mapper
            _mapper = _cqrsAdapter.CreateMapper();

            // create client
            var ch = new Channel(configuration.Url, configuration.Port, ChannelCredentials.Insecure);
            _invoker = new DefaultCallInvoker(ch);
        }

        public string Id { get; }

        public string Host => $"{_configuration.Url}:{_configuration.Port}";

        internal IEnumerable<Assembly> ContractsAssemblies => _configuration.ContractsAssemblies.ToList();

        public string GetProto()
        {
            return _cqrsAdapter.GetProto();
        }

        public async Task<GrpcResponseEnvelope<TResponse>> Execute<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException();
            }

            // timeout 
            cancellationToken = cancellationToken.AddTimeout(_configuration.TimeoutMs);

            // ch info
            var chInfo = _cqrsAdapter.ToCqrsChannelInfo().FirstOrDefault(x => x.ReqType == request.GetType() && x.RspType == typeof(TResponse));
            if (chInfo == null)
            {
                throw new ArgumentException("Invaid request type.");
            }

            // calll
            var callMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(x => x.Name == nameof(GrpcCqrsClient.CallUnaryMethodAsync))
                .MakeGenericMethod(chInfo.ReqType, chInfo.RspType, chInfo.ChReqType, chInfo.ChRspType, chInfo.ChRspEnvType);
            var execTask = callMethod.Invoke(this, new object[] { request, default(CancellationToken) }) as Task<GrpcResponseEnvelope<TResponse>>;
            var result = await execTask;
            return result;
        }

        private async Task<GrpcResponseEnvelope<TResponse>> CallUnaryMethodAsync<TRequest, TResponse, TChRequest, TChResponse, TChResponseEnvelope>(TRequest req, CancellationToken ct)
            where TRequest : class
            where TChRequest : class
            where TChResponseEnvelope : class
        {

            GrpcResponseEnvelope<TResponse> rsp = null;
            Exception exception = null;
            var reqName = req.GetType().Name;

            var watch = Stopwatch.StartNew();
            try
            {
                _logger?.LogDebug("Request {requestName} started on client {clientId} for host {host}.", reqName, Id, Host);
                var chReq = _mapper.Map<TChRequest>(req);
                _clientAspect?.BeforeExecution(req);
                var chRsp = await CallUnaryMethodChannelAsync<TChRequest, TChResponseEnvelope>(chReq, ct);
                rsp = _mapper.Map<GrpcResponseEnvelope<TResponse>>(chRsp);
                watch.Stop();

                // execution error
                if(rsp.IsExecutionError)
                {
                    _logger?.LogError("Request {requestName} failed on client {clientId} with host {host} execution error. Call duration was {durationMs}ms.", reqName, Id, Host, watch.ElapsedMilliseconds);
                    throw new GrpcClientExecutionException($"Execution error: {rsp.ErrorMessage}");
                }

                // data validation error
                if(rsp.IsValidationError)
                {
                    _logger?.LogInformation("Request {requestName} completed on client {clientId} with host {host} validation error. Call duration was {durationMs}ms.", reqName, Id, Host, watch.ElapsedMilliseconds);
                    throw new ValidationErrorResponseException(rsp.ValidationError);
                }

                // no error
                return rsp;
            }
            catch(ValidationErrorResponseException e)
            {
                throw e;
            }
            catch(Exception e)
            {
                exception = e;
                watch.Stop();
                _logger?.LogError(e, "Request {requestName} failed on client {clientId} for host {host}. Call duration was {durationMs}ms.", reqName, Id, Host, watch.ElapsedMilliseconds);

                // do not handle exceptions
                if (!_configuration.HandleExceptions)
                {
                    throw;
                }

                // handled exception
                rsp = new GrpcResponseEnvelope<TResponse>
                {
                    IsExecutionError = true,
                    ErrorMessage = e.Message
                };
                return rsp;

            } finally
            {
                _clientAspect?.AfterExecution(rsp, exception);
            }
        }

        private async Task<TChResponseEnvelope> CallUnaryMethodChannelAsync<TChRequest, TChResponseEnvelope>(TChRequest request, CancellationToken ct)
            where TChRequest : class
            where TChResponseEnvelope : class
        {

            // set call options
            var callOptions = new CallOptions(cancellationToken: ct, headers: new Metadata());
            _clientAspect?.OnCall(callOptions, request);

            // invoke
            var method = GetGrpcMethodDefinition<TChRequest, TChResponseEnvelope>();
            using (var call = _invoker.AsyncUnaryCall(method, null, callOptions, request))
            {
                return await call.ResponseAsync.ConfigureAwait(false);
            }
        }

        private Method<TChRequest, TChResponseEnvelope> GetGrpcMethodDefinition<TChRequest, TChResponseEnvelope>()
            where TChRequest : class
            where TChResponseEnvelope : class
        {
            var key = typeof(TChRequest);
            var method = _grpcMethods.ContainsKey(key) ? _grpcMethods[key] : null;
            if (method == null)
            {
                return null;
            }
            return method as Method<TChRequest, TChResponseEnvelope>;
        }

        private static object CreateGrpcMethodForCqrsChannel(CqrsChannelInfo info)
        {
            var grpcMethodFnc = typeof(GrpcMethodFactoryUtil).GetMethod(nameof(GrpcMethodFactoryUtil.CreateGrpcMethod)).MakeGenericMethod(info.ChReqType, info.ChRspEnvType);
            var grpcMethod = grpcMethodFnc.Invoke(null, new object[] { info.ServiceName, info.MethodName });
            return grpcMethod;
        }

    }
}
