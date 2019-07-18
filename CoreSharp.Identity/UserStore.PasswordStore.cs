using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim>
    {
        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Password = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(!string.IsNullOrEmpty(user.Password));
        }
    }
}
