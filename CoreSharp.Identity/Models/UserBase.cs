using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.NHibernate;
using FluentNHibernate.Conventions.Inspections;
using Newtonsoft.Json;

namespace CoreSharp.Identity.Models
{
    public abstract class UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim> : VersionedEntity, IUser, IIdentity
        where TRole : RoleBase<TRole, TRolePermission, TPermission>
        where TUser : UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim>
        where TUserRole : UserRoleBase<TUser, TRole>
        where TOrganizationRole : OrganizationRoleBase<TOrganization, TRole>
        where TOrganization : OrganizationBase<TOrganization, TRole, TUser, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim>
        where TClaim : UserClaimBase<TUser>
        where TPermission : PermissionBase
        where TRolePermission : RolePermissionBase<TRole, TRolePermission, TPermission>
    {
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
        public virtual DateTime? LockoutEnd { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual string SecurityStamp { get; set; }


        public virtual TOrganization Organization { get; set; }

        [ReadOnly(true)]
        public virtual long? OrganizationId { get; set; }

        public virtual bool Active { get; set; }


        [Include]
        public virtual ISet<TUserRole> Roles { get; set; } = new HashSet<TUserRole>();

        [Include]
        public virtual ISet<TClaim> Claims { get; set; } = new HashSet<TClaim>();

        #region IPrincipal
        public virtual bool IsInRole(string role)
        {
            return (Roles != null && Roles.Any(x => x.Role.Name == role)) || (Organization != null && Organization.Roles.Any(x => x.Role.Name == role));
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

        public virtual bool HasPermission(string permission)
        {
            var permissions = Roles.SelectMany(x => x.Role.Permissions)
                .Union(Organization.Roles.SelectMany(x => x.Role.Permissions)).Select(x => x.Permission).Distinct()
                .ToList();

            return permissions.Any(x => x.FullName == permission);
        }
    }
}
