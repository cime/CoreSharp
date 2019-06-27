using System.ComponentModel;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class UserRoleBase<TUser, TRole> : Entity
    {
        [NotNull]
        public virtual TUser User { get; set; }
        [NotNull]
        public virtual TRole Role { get; set; }

        [ReadOnly(true)]
        public virtual long UserId { get; set; }

        [ReadOnly(true)]
        public virtual long RoleId { get; set; }
    }
}
