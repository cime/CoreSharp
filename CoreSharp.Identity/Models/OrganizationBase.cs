using System.Collections.Generic;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    public abstract class OrganizationBase<TOrganization, TRole, TUser, TOrganizationRole> : Entity
        where TRole : IEntity
        where TUser : IEntity
        where TOrganization : OrganizationBase<TOrganization, TRole, TUser, TOrganizationRole>
        where TOrganizationRole : OrganizationRoleBase<TOrganization, TRole>
    {
        private ISet<TOrganizationRole> _organizationRoles;

        public virtual ISet<TOrganizationRole> OrganizationRoles
        {
            get { return _organizationRoles ?? (_organizationRoles = new HashSet<TOrganizationRole>()); }
            set { _organizationRoles = value; }
        }

        private ISet<TUser> _users;

        public virtual ISet<TUser> Users
        {
            get { return _users ?? (_users = new HashSet<TUser>()); }
            set { _users = value; }
        }
    }
}
