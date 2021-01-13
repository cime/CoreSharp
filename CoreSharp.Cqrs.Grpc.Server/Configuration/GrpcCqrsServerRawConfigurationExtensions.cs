using System;
using System.Linq;
using CoreSharp.Common.Extensions;
using CoreSharp.Cqrs.Grpc.Mapping;

namespace CoreSharp.Cqrs.Grpc.Server
{
    public static class GrpcCqrsServerRawConfigurationExtensions
    {
        public static GrpcCqrsServerConfiguration ToConfiguration(this GrpcCqrsServerRawConfiguration raw)
        {
            var mapType = !string.IsNullOrWhiteSpace(raw?.MapperValidator) ? Type.GetType(raw.MapperValidator) : null;
            var mapperValidator = mapType.GetAllInterfaces().Any(x => x == typeof(IPropertyMapValidator)) ? mapType : null;

            var cfg = new GrpcCqrsServerConfiguration
            {
                Url = raw.Url,
                Port = raw.Port,
                TimeoutMs = raw.TimeoutMs,
                ServiceNamePrefix = raw.ServiceNamePrefix,
                ContractsAssemblies = raw.ContractsAssemblies.ToAssemblies(),
                RegisteredOnly = raw.RegisteredOnly,
                MapperValidator = mapperValidator,
                ServerId = raw.ServerId
            };
            return cfg;
        }
    }
}
