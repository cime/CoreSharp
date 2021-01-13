using System;
using AutoMapper;

namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public class DateTimeFromChannelConverter : ITypeConverter<long, DateTime>
    {

        public DateTime Convert(long source, DateTime destination, ResolutionContext context)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(source);
            DateTime dateTime = new DateTime(1970, 1, 1,0,0,0, DateTimeKind.Utc) + time;
            return dateTime.ToLocalTime();
        }
    }
}
