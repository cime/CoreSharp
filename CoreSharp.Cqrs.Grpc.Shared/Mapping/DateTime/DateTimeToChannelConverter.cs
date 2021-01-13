using System;
using AutoMapper;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class DateTimeToChannelConverter : ITypeConverter<DateTime, long>
    {

        public long Convert(DateTime source, long destination, ResolutionContext context)
        {
            var msTimestamp = new DateTimeOffset(source).ToUnixTimeMilliseconds();
            return msTimestamp;
        }
    }
}
