using System.ComponentModel;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class OrganizationRoleBase<TOrganization, TRole> : Entity
    {
        [NotNull]
        public virtual TOrganization Organization { get; set; }
        [NotNull]
        public virtual TRole Role { get; set; }

        [ReadOnly(true)]
        public virtual long OrganizationId { get; set; }

        [ReadOnly(true)]
        public virtual long RoleId { get; set; }
    }
}
