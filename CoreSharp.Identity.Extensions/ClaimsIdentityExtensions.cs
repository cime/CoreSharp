using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CoreSharp.Identity.Extensions
{
    public static class ClaimsIdentityExtensions
    {

        public static bool HasPermission(this ClaimsIdentity claimsIdentity, string permission)
        {
            return claimsIdentity.GetPermissions().Contains(permission);
        }

        public static bool IsSystemUser(this ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.FirstOrDefault(o => o.Type == "system_user")?.Value?.ToLowerInvariant() == "true";
        }

        public static bool HasAnyPermission(this ClaimsIdentity claimsIdentity, params string[] permissions)
        {
            return claimsIdentity.GetPermissions().Intersect(permissions).Any();
        }

        public static bool HasAllPermissions(this ClaimsIdentity claimsIdentity, params string[] permissions)
        {
            return claimsIdentity.GetPermissions().Intersect(permissions).Count() == permissions.Length;
        }

        public static IEnumerable<string> GetPermissions(this ClaimsIdentity claimsIdentity)
        {
            var result = new string[] { };

            if (claimsIdentity.HasClaim(x => x.Type == "permissions"))
            {
                var permissions = claimsIdentity.Claims.Single(x => x.Type == "permissions").Value;

                if (!string.IsNullOrEmpty(permissions))
                {
                    result = permissions.Split(',');
                }
            }

            return result;
        }
    }
}
