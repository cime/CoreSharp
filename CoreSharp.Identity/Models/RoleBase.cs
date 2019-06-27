using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class RoleBase : Entity, IRole
    {
        [NotNullOrEmpty]
        public virtual string Name { get; set; }

        //TODO: references to UserRoles and OrganizationRoles
    }
}
