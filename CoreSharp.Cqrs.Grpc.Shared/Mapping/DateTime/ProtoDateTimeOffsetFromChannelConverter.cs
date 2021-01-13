using System;
using AutoMapper;
using CoreSharp.Cqrs.Grpc.Common;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class ProtoDateTimeOffsetFromChannelConverter : ITypeConverter<ProtoDateTimeOffset, DateTimeOffset>
    {

        public DateTimeOffset Convert(ProtoDateTimeOffset source, DateTimeOffset destination, ResolutionContext context)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(source.Value);
            DateTime dateTime = new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc) + time;
            var dateTimeOffset = new DateTimeOffset(dateTime).ToOffset(TimeSpan.FromMinutes(source.Offset));
            return dateTimeOffset;
        }
    }
}
