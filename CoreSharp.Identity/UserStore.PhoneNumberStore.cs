using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim> : IUserPhoneNumberStore<TUser>
    {
        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumber = phoneNumber;

            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumberConfirmed = confirmed;

            return Task.CompletedTask;
        }
    }
}
