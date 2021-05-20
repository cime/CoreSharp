using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Contracts;
using CoreSharp.Cqrs.Grpc.Mapping;
using CoreSharp.Cqrs.Grpc.Serialization;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Common
{
    public static class CqrsInfoExtensions
    {

        public static IReadOnlyDictionary<Type, Type> BuildChannelContracts(this IEnumerable<CqrsInfo> cqrs)
        {
            // create new types with proto attributes 
            var contractsBuilder = new ContractsBuilder();
            cqrs.ForEach(x => {
                contractsBuilder.AddType(x.ReqType);
                if(x.RspType != null)
                {
                    contractsBuilder.AddType(x.RspType);
                    var envType = typeof(GrpcResponseEnvelope<>).MakeGenericType(x.RspType);
                    contractsBuilder.AddType(envType);
                }
            });
            contractsBuilder.Build();

            // types map 
            var typesMap = contractsBuilder.GetTypesMap();
            return typesMap;
        }

        public static IEnumerable<CqrsChannelInfo> ToCqrsChannelInfo(this IEnumerable<CqrsInfo> cqrs, IReadOnlyDictionary<Type, Type> channelContracts, string serviceNamePrefix = "Cqrs_Grpc_")
        {
            // channel cqrs
            var channelCqrs = cqrs.Select(x => new CqrsChannelInfo(
                x.ReqType,
                $"{serviceNamePrefix}{x.ServiceName}",
                x.MethodName,
                x.Formatter,
                x.IsQuery,
                x.IsCommand,
                x.IsAsync,
                x.IsAuthorize,
                x.RspType,
                channelContracts[x.ReqType],
                x.RspType != null ? channelContracts[x.RspType] : null,
                x.RspType != null ? channelContracts[typeof(GrpcResponseEnvelope<>).MakeGenericType(x.RspType)] : null,
                x.Permissions
            )).ToList();
            return channelCqrs;
        }

        public static IMapper ToMapper(this IReadOnlyDictionary<Type, Type> chContracts, IPropertyMapValidator validator = null)
        {
            return MappingBuilder.CreateMapper(chContracts, validator);
        }

        public static string ToProto<TSerializer>(this IEnumerable<CqrsChannelInfo> cqrs) 
            where TSerializer: ISerializer
        {
            var serializer = Activator.CreateInstance<TSerializer>();
            return serializer.GetProto(cqrs);
        }

    }
}
