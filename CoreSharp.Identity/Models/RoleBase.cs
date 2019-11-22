using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class RoleBase<TRole, TRolePermission, TPermission> : Entity, IRole
        where TPermission : PermissionBase
        where TRolePermission : RolePermissionBase<TRole, TRolePermission, TPermission>
        where TRole : RoleBase<TRole, TRolePermission, TPermission>
    {
        [NotNull]
        public virtual string Name { get; set; }

        [NotNull]
        public virtual string NormalizedName { get; set; }

        [Include]
        public virtual ISet<TRolePermission> Permissions { get; set; } = new HashSet<TRolePermission>();
    }
}
