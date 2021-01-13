using System;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class ProtoDateTimeOffsetToChannelConverter : ITypeConverter<DateTimeOffset, ProtoDateTimeOffset>
    {

        public ProtoDateTimeOffset Convert(DateTimeOffset source, ProtoDateTimeOffset destination, ResolutionContext context)
        {
            var value = source.ToUnixTimeMilliseconds();
            return new ProtoDateTimeOffset
            {
                Value = value,
                Offset = (int)source.Offset.TotalMinutes
            };
        }
    }
}
