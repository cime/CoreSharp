using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class ProtoNullableFromChannelConverter<TSource> : IValueConverter<ProtoNullable<TSource>, TSource?>
        where TSource : struct
    {
        public TSource? Convert(ProtoNullable<TSource> sourceMember, ResolutionContext context)
        {
            if(sourceMember != null && sourceMember.HasValue)
            {
                return sourceMember.Value;
            }
            return null;
        }
    }
}
