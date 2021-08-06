namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public class GrpcCqrsAspNetCoreBearerConfiguration
    {
        public string Secret { get; set; }

        public string JwtIssuer { get; set; } = "coresharp-grpc";

        public int ClockSkewSeconds { get; set; } = 30;

        public bool RequireHttpsMetadata { get; set; }

        public bool SaveToken { get; set; }

    }
}
