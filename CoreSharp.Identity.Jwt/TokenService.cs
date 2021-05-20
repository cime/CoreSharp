using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;

namespace CoreSharp.Identity.Jwt
{
    public class TokenService
    {
        private readonly TokenFactory _factory;
        private readonly TokenConfiguration _configuration;
        private readonly ConcurrentDictionary<string, TokenData> _tokens = new ConcurrentDictionary<string, TokenData>();

        public TokenService(TokenConfiguration configuration)
        {
            _factory = new TokenFactory(configuration);
            _configuration = configuration;
        }

        public string GetUserToken(string user)
        {

            var key = "token-" + user;

            // get existing token 
            var tokenData = _tokens.GetOrAdd(key, x => CreateNewtoken(user));
            
            // token not valid 
            if(tokenData == null || tokenData.ValidUntil < DateTime.UtcNow)
            {
                // create new one 
                tokenData = _tokens.AddOrUpdate(key, x => CreateNewtoken(user), (x, old) => CreateNewtoken(user));
            }

            return tokenData?.Payload;
        }

        public string GetDefaultUserToken()
        {
            var user = _configuration.Users?.FirstOrDefault()?.Name;
            if(string.IsNullOrWhiteSpace(user))
            {
                return null;
            }

            return GetUserToken(user);
        }

        private TokenData CreateNewtoken(string user)
        {

            var claims = _configuration.Users?.FirstOrDefault(x => x.Name == user)?.Claims
                .Select(x => new Claim(x.Key, x.Value));
            if (claims == null || !claims.Any())
            {
                return null;
            }

            var targetClaims = claims.ToList();
            targetClaims.Add(new Claim(ClaimTypes.Name, user));
            targetClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, _configuration.AuthMethod));

            var payload = _factory.GenerateTokenForClaims(targetClaims);
            var validUnit = DateTime.UtcNow.AddSeconds(_configuration.JwtExpireSeconds / 2);
            return new TokenData
            {
                Payload = payload,
                ValidUntil = validUnit
            };

        }
    }
}
