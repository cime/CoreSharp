using System;
using System.Linq;
using System.Reflection;
using CoreSharp.Cqrs.Grpc.Serialization;
using Grpc.Core;

namespace CoreSharp.Cqrs.Grpc.Common
{

    public static class GrpcMethodFactoryUtil
    {

        public static object CreateGrpcMethodGeneric(CqrsChannelInfo info)
        {
            var method = typeof(GrpcMethodFactoryUtil).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(x => x.Name == nameof(GrpcMethodFactoryUtil.CreateGrpcMethod))
                .MakeGenericMethod(info.ChReqType, info.ChRspEnvType);

            var grpcMethod = method.Invoke(null, new object[] { info.ServiceName, info.MethodName });
            return grpcMethod;

        }

        public static Method<TChRequest, TChEnvResponse> CreateGrpcMethod<TChRequest, TChEnvResponse>(string serviceName, string methodName)
            where TChRequest : class
            where TChEnvResponse : class
        {
            return CreateGrpcMethodWithCustomSerializer<TChRequest, TChEnvResponse, ProtoBufSerializer>(serviceName, methodName);
        }

        public static Method<TChRequest, TChEnvResponse> CreateGrpcMethodWithCustomSerializer<TChRequest, TChEnvResponse, TSerializer>(string serviceName, string methodName)
            where TChRequest : class
            where TChEnvResponse : class
            where TSerializer : ISerializer
        {
            // create serializer 
            var serializer = Activator.CreateInstance<TSerializer>();

            // create method
            var method = new Method<TChRequest, TChEnvResponse>(
                type: MethodType.DuplexStreaming,
                serviceName: serviceName,
                name: methodName,
                requestMarshaller: Marshallers.Create(
                    obj => serializer.Serialize(obj),
                    bytes => serializer.Deserialize<TChRequest>(bytes)),
                responseMarshaller: Marshallers.Create(
                    obj => serializer.Serialize(obj),
                    bytes => serializer.Deserialize<TChEnvResponse>(bytes)));

            // return
            return method;
        }

    }
}
