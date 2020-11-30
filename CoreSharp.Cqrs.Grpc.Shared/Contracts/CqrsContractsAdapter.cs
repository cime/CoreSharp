using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Cqrs.Grpc.Mapping;
using CoreSharp.Cqrs.Grpc.Serialization;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Grpc.Contracts
{
    public class CqrsContractsAdapter
    {
        private readonly IEnumerable<CqrsInfo> _cqrs;
        private readonly IReadOnlyDictionary<Type, Type> _contracts;
        private readonly IEnumerable<CqrsChannelInfo> _chCqrs;

        public CqrsContractsAdapter(IEnumerable<CqrsInfo> cqrs, string serviceNamePrefix = "Cqrs_Grpc_")
        {
            _cqrs = cqrs;
            _contracts = cqrs.BuildChannelContracts();
            _chCqrs = _cqrs.ToCqrsChannelInfo(_contracts, serviceNamePrefix); ;

        }

        public IEnumerable<CqrsChannelInfo> ToCqrsChannelInfo()
        {
            return _chCqrs;
        }

        public IMapper CreateMapper(IPropertyMapValidator validator = null)
        {
            var mapper = _contracts.ToMapper(validator);
            return mapper;
        }

        public string GetProto(Assembly assemlby = null) {

            return _chCqrs.Where(x => assemlby == null || x.ReqType.Assembly == assemlby).ToProto<ProtoBufSerializer>();
        }
    }
}
