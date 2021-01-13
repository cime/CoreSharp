using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class ProtoNullableFromChannelConverter<TValue, TChValue> : ITypeConverter<ProtoNullable<TChValue>, TValue?>
        where TValue : struct
        where TChValue: struct
    {
        public TValue? Convert(ProtoNullable<TChValue> source, TValue? destination, ResolutionContext context)
        {
            if (source == null || !source.HasValue)
            {
                return null;
            }

            var value = context.Mapper.Map<TValue>(source.Value);
            return value;
        }
    }

}
