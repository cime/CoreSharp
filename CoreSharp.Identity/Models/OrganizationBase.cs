using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class OrganizationBase<TOrganization, TRole, TUser, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim> : VersionedEntityWithUser<TUser>
        where TRole : RoleBase<TRole, TRolePermission, TPermission>
        where TUser : UserBase<TUser, TRole, TOrganization, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim>
        where TUserRole : UserRoleBase<TUser, TRole>
        where TPermission : PermissionBase
        where TOrganization : OrganizationBase<TOrganization, TRole, TUser, TUserRole, TOrganizationRole, TRolePermission, TPermission, TClaim>
        where TOrganizationRole : OrganizationRoleBase<TOrganization, TRole>
        where TRolePermission : RolePermissionBase<TRole, TRolePermission, TPermission>
        where TClaim : UserClaimBase<TUser>
    {
        [Include]
        public virtual ISet<TOrganizationRole> Roles { get; set; } = new HashSet<TOrganizationRole>();

        [Include]
        public virtual ISet<TUser> Users { get; set; } = new HashSet<TUser>();
    }
}
