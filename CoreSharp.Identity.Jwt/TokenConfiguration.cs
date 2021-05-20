using System.Collections.Generic;

namespace CoreSharp.Identity.Jwt
{
    public class TokenConfiguration
    {
        public string Secret { get; set; }

        public string JwtIssuer { get; set; } = "coresharp-grpc";

        public string JwtAudience { get; set; } = "coresharp-grpc";

        public string AuthMethod { get; set; } = "coresharp-grpc";

        public float JwtExpireSeconds { get; set; } = 120;

        public IEnumerable<TokenConfigurationUser> Users { get; set; }
    }
}
