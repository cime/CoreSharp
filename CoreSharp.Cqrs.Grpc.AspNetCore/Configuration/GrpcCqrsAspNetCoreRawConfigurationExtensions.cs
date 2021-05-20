using System;
using CoreSharp.Common.Extensions;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public static class GrpcCqrsAspNetCoreRawConfigurationExtensions
    {
        public static GrpcCqrsAspNetCoreConfiguration ToConfiguration(this GrpcCqrsAspNetCoreRawConfiguration raw)
        {
            var cfg = new GrpcCqrsAspNetCoreConfiguration
            {
                ContractsAssemblies = raw.ContractsAssemblies.ToAssemblies(),
                ServiceNamePrefix = raw.ServiceNamePrefix,
                TimeoutMs = raw.TimeoutMs,
                ServerId = raw.ServerId,
                ExposeProto = raw.ExposeProto,
                MapperValidator = !string.IsNullOrWhiteSpace(raw.MapperValidator) ? Type.GetType(raw.MapperValidator) : null
            };
            return cfg;
        }
    }
}
