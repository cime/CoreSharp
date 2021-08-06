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
using CoreSharp.Identity.Jwt;
using CoreSharp.Validation;
using Grpc.Core;
#if NETSTANDARD2_1 || NET5_0
using Grpc.Net.Client;
#endif
using Microsoft.Extensions.Logging;

namespace CoreSharp.Cqrs.Grpc.Client
{

    public class GrpcCqrsClient
    {

        private readonly CallInvoker _invoker;

        private readonly IMapper _mapper;

        private readonly TokenService _tokenService;

        private readonly Dictionary<Type, object> _grpcMethods;

        private readonly CqrsContractsAdapter _cqrsAdapter;

        private readonly GrpcCqrsClientConfiguration _configuration;

        private ILogger<GrpcCqrsClient> _logger;

        private readonly IEnumerable<IGrpcClientAspect> _clientAspects;

        public GrpcCqrsClient(GrpcCqrsClientConfiguration configuration, 
            ILogger<GrpcCqrsClient> logger, 
            IEnumerable<IGrpcClientAspect> clientAspects = null)
        {

            _configuration = configuration;
            _logger = logger;
            _clientAspects = clientAspects?.ToList();

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

            // token service
            _tokenService = (_configuration.TokenConfiguration != null && _configuration.AuthorizationType == EnumChannelAuthorizationType.Token) 
                ? new TokenService(_configuration.TokenConfiguration) : null;

            // create client invoker
            _invoker = CreateCallInvoker(configuration);
        }

        public string Id { get; }

        public string Host => $"{_configuration.Url}:{_configuration.Port}";

        internal IEnumerable<Assembly> ContractsAssemblies => _configuration.ContractsAssemblies.ToList();

        public string GetProto()
        {
            return _cqrsAdapter.GetProto();
        }

        public async Task<GrpcResponseEnvelope<TResponse>> Execute<TRequest, TResponse>(TRequest request, GrpcCqrsCallOptions callOptions = null, CancellationToken cancellationToken = default)
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
            var execTask = callMethod.Invoke(this, new object[] { request, callOptions, cancellationToken }) as Task<GrpcResponseEnvelope<TResponse>>;
            var result = await execTask;
            return result;
        }

        private async Task<GrpcResponseEnvelope<TResponse>> CallUnaryMethodAsync<TRequest, TResponse, TChRequest, TChResponse, TChResponseEnvelope>(TRequest req, GrpcCqrsCallOptions options, CancellationToken ct)
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
                _clientAspects?.ForEach(x => x.BeforeExecution(req));
                var chRsp = await CallUnaryMethodChannelAsync<TChRequest, TChResponseEnvelope>(chReq, options, ct);
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

                // special case for unauthenticated exception
                if (_configuration.HandleUnauthenticated && e is RpcException && (e as RpcException).StatusCode == StatusCode.Unauthenticated)
                {
                    throw new UnauthorizedAccessException("GRPC call unauthenticated.");
                }

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
                _clientAspects?.ForEach(x => x.AfterExecution(rsp, exception));
            }
        }

        private async Task<TChResponseEnvelope> CallUnaryMethodChannelAsync<TChRequest, TChResponseEnvelope>(TChRequest request, GrpcCqrsCallOptions options, CancellationToken ct)
            where TChRequest : class
            where TChResponseEnvelope : class
        {

            // set call options
            var callOptions = new CallOptions(cancellationToken: ct, headers: new Metadata());

            // use default options if not provided
            if(options == null && _configuration.DefaultCallOptions != null)
            {
                options = _configuration.DefaultCallOptions;
            }

            // add local token
            if(options != null && options.AddInternalAuthorization)
            {
                var token = _tokenService?.GetDefaultUserToken();
                if (!string.IsNullOrWhiteSpace(token))
                {

                    var key = "authorization";

                    // remove existing header
                    var existingAuth = callOptions.Headers.FirstOrDefault(x => x.Key == key);
                    if (existingAuth != null)
                    {
                        callOptions.Headers.Remove(existingAuth);
                    }

                    // add token
                    callOptions.Headers.Add(key, "Bearer " + token);
                }
            }

            // aspect options 
            _clientAspects?.ForEach(x => x.OnCall(callOptions, request, options));

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

#if NETSTANDARD2_1 || NET5_0

        private CallInvoker CreateCallInvoker(GrpcCqrsClientConfiguration configuration)
        {
            _logger?.LogInformation("Creating insecure net client.");
            var ch = GrpcChannel.ForAddress($"http://{configuration.Url}:{configuration.Port}");
            return ch.CreateCallInvoker();
        }

#else

        private CallInvoker CreateCallInvoker(GrpcCqrsClientConfiguration configuration)
        {
            _logger?.LogInformation("Creating insecure client.");
            var ch = new Channel(configuration.Url, configuration.Port, ChannelCredentials.Insecure);
            return new DefaultCallInvoker(ch);
        }

#endif

        private static object CreateGrpcMethodForCqrsChannel(CqrsChannelInfo info)
        {
            var grpcMethodFnc = typeof(GrpcMethodFactoryUtil).GetMethod(nameof(GrpcMethodFactoryUtil.CreateGrpcMethod)).MakeGenericMethod(info.ChReqType, info.ChRspEnvType);
            var grpcMethod = grpcMethodFnc.Invoke(null, new object[] { info.ServiceName, info.MethodName });
            return grpcMethod;
        }

    }
}
