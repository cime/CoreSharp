using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public class GrpcClientManager : IGrpcClientManager
    {
        private readonly ICollection<GrpcCqrsClient> _clients;

        public GrpcClientManager(ICollection<GrpcCqrsClient> clients)
        {
            _clients = clients;
        }

        public bool ExistsClientFor<T>(T obj)
        {
            var assemlby = obj?.GetType()?.Assembly;
            var status = assemlby != null &&  _clients.Any(x => x.ContractsAssemblies.Contains(assemlby));
            return status;
        }

        public GrpcCqrsClient GetClientFor<T>(T obj)
        {
            var assemlby = obj?.GetType()?.Assembly;
            var client = assemlby != null ? _clients.First(x => x.ContractsAssemblies.Contains(assemlby)) : null;
            return client;
        }

        public IEnumerable<string> GetProtos()
        {
            var protos = _clients.Select(x => x.GetProto()).ToList();
            return protos;
        }
    }
}
