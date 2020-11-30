using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{

    public class ProtoNullableToChannelConverter<TSource> : IValueConverter<TSource?, ProtoNullable<TSource>>
        where TSource : struct
    {

        public ProtoNullable<TSource> Convert(TSource? sourceMember, ResolutionContext context)
        {
            var dst = new ProtoNullable<TSource>
            {
                HasValue = sourceMember.HasValue,
            };
            if(sourceMember.HasValue)
            {
                dst.Value = sourceMember.Value;
            }
            return dst;
        }
    }
}
