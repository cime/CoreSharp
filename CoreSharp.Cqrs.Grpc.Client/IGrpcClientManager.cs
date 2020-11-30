using System.Collections.Generic;

namespace CoreSharp.Cqrs.Grpc.Client
{
    public interface IGrpcClientManager
    {

        GrpcCqrsClient GetClientFor<T>(T obj);

        bool ExistsClientFor<T>(T obj);

        IEnumerable<string> GetProtos();

    }
}
