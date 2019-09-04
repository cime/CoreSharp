using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NHibernate.Linq;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim> : IUserSecurityStampStore<TUser>
    {

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }
    }
}

