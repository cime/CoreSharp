using System.Runtime.Serialization;

namespace CoreSharp.Cqrs.Grpc.Common
{
    [DataContract]
    public struct ProtoDateTimeOffset
    {
        [DataMember(Order = 1)]
        public long Value { get; set; }

        [DataMember(Order = 2)]
        public int Offset { get; set; }
    }

}
