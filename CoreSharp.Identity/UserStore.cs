using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Identity.Models;
using Microsoft.AspNetCore.Identity;
using NHibernate;
using NHibernate.Linq;

namespace CoreSharp.Identity
{
    public partial class UserStore<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TClaim> : IUserPasswordStore<TUser>, IQueryableUserStore<TUser>
        where TRole : RoleBase
        where TUserRole : UserRoleBase<TUser, TRole>, new()
        where TOrganizationRole : OrganizationRoleBase<TOrganization, TRole>
        where TOrganization : OrganizationBase<TOrganization, TRole, TUser, TOrganizationRole>
        where TUser : UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TClaim>, new()
        where TClaim : UserClaimBase<TUser>, new()
    {
        private readonly ISession _session;

        public IQueryable<TUser> Users => _session.Query<TUser>();

        public UserStore(ISession session)
        {
            _session = session;
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id.ToString(CultureInfo.InvariantCulture));
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _session.SaveOrUpdateAsync(user, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _session.SaveOrUpdateAsync(user, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _session.DeleteAsync(user, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var id = ConvertIdFromString(userId);

            return _session.Query<TUser>()
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (normalizedUserName == null)
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            return _session.Query<TUser>()
                .Where(x => x.NormalizedUserName == normalizedUserName)
                .SingleOrDefaultAsync(cancellationToken);
        }

        private static long ConvertIdFromString(string id)
        {
            if (id == null)
            {
                return default;
            }

            return long.Parse(id, CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {

        }
    }
}
