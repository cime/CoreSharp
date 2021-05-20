using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.Tests.Grpc.Samples.Models
{
    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int? Status { get; set; }

        public int? Roles { get; set; }

        public DateTimeOffset LocationTime { get; set; }

        public DateTimeOffset? LastLogin { get; set; }

        public DateTime UserCreated { get; set; }

        public DateTime? UserActivated { get; set; }

        public IDictionary<string,Application> Applications { get; set; }
    }
}
