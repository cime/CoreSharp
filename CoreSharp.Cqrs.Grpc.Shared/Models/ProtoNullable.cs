using System.Runtime.Serialization;

namespace CoreSharp.Cqrs.Grpc.Common
{
    [DataContract]
    public class ProtoNullable<T>
        where T : struct
    {
        [DataMember(Order = 1)]
        public T Value { get; set; }

        [DataMember(Order = 2)]
        public bool HasValue { get; set; }
    }
}
