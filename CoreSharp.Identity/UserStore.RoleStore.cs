using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TClaim> : IUserRoleStore<TUser>
    {
        public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            var userRole = new TUserRole()
            {
                User = user,
                Role = _session.Query<TRole>().Single(x => x.Name == roleName)
            };

            user.UserRoles.Add(userRole);

            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            var userRole = user.UserRoles.FirstOrDefault(x => x.Role.Name == roleName);

            if (userRole != null)
            {
                user.UserRoles.Remove(userRole);

                _session.Delete(userRole);
            }

            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<string>)user.UserRoles.Select(x => x.Role.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return Task.FromResult(user.IsInRole(roleName));
        }

        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return Task.FromResult((IList<TUser>)_session.Query<TUser>().Where(x => x.UserRoles.Any(r => r.Role.Name == roleName)).ToList());
        }
    }
}
