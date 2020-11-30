using System.Collections.Generic;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Serialization
{
    public interface ISerializer
    {

        byte[] Serialize<T>(T input);

        T Deserialize<T>(byte[] input);

        string GetProto(IEnumerable<CqrsChannelInfo> cqrs);
    }
}
