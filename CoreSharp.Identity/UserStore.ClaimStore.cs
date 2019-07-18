using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim> : IUserClaimStore<TUser>
    {
        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<Claim>)user.Claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList());
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userClaims = claims.Select(x => new TClaim()
            {
                ClaimType = x.Type,
                ClaimValue = x.Value,
                User = user
            });

            foreach (var userClaim in userClaims)
            {
                user.Claims.Add(userClaim);
            }

            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userClaim = user.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            if (userClaim == null)
            {
                userClaim = new TClaim() {User = user};
                user.Claims.Add(userClaim);

                _session.SaveOrUpdate(userClaim);
            }

            userClaim.ClaimType = newClaim.Type;
            userClaim.ClaimValue = newClaim.Value;

            return Task.CompletedTask;
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claimsToBeRemoved = user.Claims.Where(x => claims.Any(c => c.Type == x.ClaimType && c.Value == x.ClaimValue)).ToList();

            foreach (var claim in claimsToBeRemoved)
            {
                user.Claims.Remove(claim);
                _session.Delete(claim);
            }

            return Task.CompletedTask;
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            return Task.FromResult((IList<TUser>)_session.Query<TClaim>().Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Select(x => x.User).ToList());
        }
    }
}
