using System;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    [Ignore]
    [Serializable]
    public abstract class RolePermissionBase<TRole, TRolePermission, TPermission> : Entity
        where TRole : RoleBase<TRole, TRolePermission, TPermission>
        where TRolePermission : RolePermissionBase<TRole, TRolePermission, TPermission>
        where TPermission : PermissionBase
    {
        public virtual TRole Role { get; set; }
        public virtual TPermission Permission { get; set; }
    }
}
