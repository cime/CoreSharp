using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.NHibernate;
using Newtonsoft.Json;

namespace CoreSharp.Identity.Models
{
    public abstract class UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TClaim> : VersionedEntity, IUser, IIdentity
        where TRole : RoleBase
        where TUser : UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TClaim>
        where TUserRole : UserRoleBase<TUser, TRole>
        where TOrganizationRole : OrganizationRoleBase<TOrganization, TRole>
        where TOrganization : OrganizationBase<TOrganization, TRole, TUser, TOrganizationRole>
        where TClaim : UserClaimBase<TUser>
    {
        public virtual long Id { get; }

        [NotNullOrEmpty]
        [Unique]
        [Length(50)]
        public virtual string UserName { get; set; }

        [NotNullOrEmpty]
        [Unique]
        [Length(50)]
        public virtual string NormalizedUserName { get; set; }

        [NotNullOrEmpty]
        [Email]
        public virtual string Email { get; set; }

        [NotNullOrEmpty]
        [Email]
        public virtual string NormalizedEmail { get; set; }

        public virtual string Password { get; set; }

        public virtual bool EmailConfirmed { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual DateTimeOffset? LockoutEnd { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual int AccessFailedCount { get; set; }

        public virtual TOrganization Organization { get; set; }

        [ReadOnly(true)]
        public virtual long? OrganizationId { get; set; }

        public virtual bool Active { get; set; }


        private ISet<TUserRole> _userRoles;

        public virtual ISet<TUserRole> UserRoles
        {
            get { return _userRoles ?? (_userRoles = new HashSet<TUserRole>()); }
            set { _userRoles = value; }
        }

        private ISet<TClaim> _claims;

        public virtual ISet<TClaim> Claims
        {
            get { return _claims ?? (_claims = new HashSet<TClaim>()); }
            set { _claims = value; }
        }


        #region IPrincipal
        public virtual bool IsInRole(string role)
        {
            return (UserRoles != null && UserRoles.Any(x => x.Role.Name == role)) || (Organization != null && Organization.OrganizationRoles.Any(x => x.Role.Name == role));
        }

        [Ignore]
        [JsonIgnore]
        public virtual IIdentity Identity { get { return this; } }
        #endregion

        #region IIdentity
        [Ignore]
        [JsonIgnore]
        public virtual string Name { get { return UserName; } }
        [Ignore]
        [JsonIgnore]
        public virtual string AuthenticationType { get { return ""; } }
        [Ignore]
        [JsonIgnore]
        public virtual bool IsAuthenticated { get { return !IsTransient(); } }
        #endregion
    }
}
