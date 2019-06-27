using System.ComponentModel;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class UserClaimBase<TUser> : Entity, IClaim
    {
        public virtual string ClaimType { get; set; }

        public virtual string ClaimValue { get; set; }

        [NotNull]
        public virtual TUser User { get; set; }

        [ReadOnly(true)]
        public virtual long UserId { get; set; }
    }
}
