using System.Security.Claims;
using CoreSharp.Identity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreSharp.Cqrs.Mvc
{
    public class AuthorizePermissionsAttribute : AuthorizeAttribute, IAuthorizationFilter
    {

        private readonly string[] _permissions;

        public AuthorizePermissionsAttribute(string[] permissions)
        {
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity as ClaimsIdentity;
            if(identity == null  || !identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
            }

            if(_permissions != null && !identity.IsSystemUser() && !identity.HasAnyPermission(_permissions))
            {
                context.Result = new UnauthorizedResult();
            }

        }

    }
}
