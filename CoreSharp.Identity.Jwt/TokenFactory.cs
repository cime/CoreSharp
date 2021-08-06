using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoreSharp.Identity.Jwt
{
    public class TokenFactory
    {

        private readonly SecurityTokenHandler _tokenHandler;
        private readonly string _algorithm;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly TokenConfiguration _configuration;

        public TokenFactory(TokenConfiguration configuration)
        {
            _tokenHandler = new JwtSecurityTokenHandler();
            _algorithm = SecurityAlgorithms.HmacSha256; // TODO: configurable
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Secret));
            _configuration = configuration;
        }

        public string GenerateTokenForClaims(IEnumerable<Claim> claims)
        {

            var token = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _configuration.JwtIssuer,
                Audience = _configuration.JwtAudience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddSeconds(_configuration.JwtExpireSeconds),
                SigningCredentials = new SigningCredentials(_securityKey, _algorithm)
            });

            return _tokenHandler.WriteToken(token);
        }
    }
}
