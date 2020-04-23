using System.Security.Principal;

namespace CoreSharp.NHibernate
{
    public interface IIdentityAccessor
    {
        IIdentity Identity { get; }
    }
}
