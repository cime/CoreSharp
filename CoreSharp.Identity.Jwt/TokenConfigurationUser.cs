using System.Collections.Generic;

namespace CoreSharp.Identity.Jwt
{
    public class TokenConfigurationUser
    {
        public string Name { get; set; }

        public IDictionary<string, string> Claims { get; set; }
    }
}
