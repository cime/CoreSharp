using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CoreSharp.Cqrs.Grpc.AspNetCore
{
    public static class GrpcCqrsAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddCoreSharpGrpc(this AuthenticationBuilder builder, GrpcCqrsAspNetCoreBearerConfiguration configuration)
        {
            // register confiuration 
            builder.Services.AddSingleton(x => configuration);

            builder.AddJwtBearer($"Bearer-{configuration.JwtIssuer}", x => {
                x.RequireHttpsMetadata = configuration.RequireHttpsMetadata;
                x.SaveToken = configuration.SaveToken;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(configuration.Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(configuration.ClockSkewSeconds)
                };
            });

            return builder;
        }

        public static IApplicationBuilder UseCoreSharpAuthentication(this IApplicationBuilder builder)
        {
            return builder.Use(async (context, next) =>
            {
                var cfg = context.RequestServices.GetService<GrpcCqrsAspNetCoreBearerConfiguration>();

                if (!context.User.Identity.IsAuthenticated)
                {
                    var key = "Authorization";
                    var authParts = (context.Request.Headers.TryGetValue(key, out var sv) ? sv.ToString() : null)?.Split(" ");
                    var token = authParts?.Length > 1 ? authParts[1] : null;
                    string scheme = null;
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        try
                        {
                            var jwtToken = new JwtSecurityToken(token);
                            scheme = jwtToken.Issuer == cfg.JwtIssuer ? $"Bearer-{jwtToken.Issuer}" : null;
                        }
                        catch (Exception)
                        {
                        }
                    }

                    // validate against selected scheme
                    if (!string.IsNullOrWhiteSpace(scheme))
                    {
                        var result = await context.AuthenticateAsync(scheme);
                        if (result.Succeeded)
                        {
                            context.User = result.Principal;
                        }
                    }
                }

                // continue
                await next();
            });

        }
    }
}
