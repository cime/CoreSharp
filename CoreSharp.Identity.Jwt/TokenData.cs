using System;

namespace CoreSharp.Identity.Jwt
{
    internal class TokenData
    {
        public string Payload { get; set; }

        public DateTime ValidUntil { get; set; }
    }
}
