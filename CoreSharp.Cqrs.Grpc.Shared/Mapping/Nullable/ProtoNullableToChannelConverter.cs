using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{

    public class ProtoNullableToChannelConverter<TValue, TChValue> : ITypeConverter<TValue?, ProtoNullable<TChValue>>
        where TValue : struct
        where TChValue : struct
    {

        public ProtoNullable<TChValue> Convert(TValue? source, ProtoNullable<TChValue> destination, ResolutionContext context)
        {
            if (!source.HasValue)
            {
                return new ProtoNullable<TChValue>
                {
                    HasValue = false
                };
            }

            var chValue = context.Mapper.Map<TChValue>(source.Value);

            var dst = new ProtoNullable<TChValue>
            {
                HasValue = source.HasValue,
            };
            if (source.HasValue)
            {
                dst.Value = chValue;
            }
            return dst;
        }
    }

}
