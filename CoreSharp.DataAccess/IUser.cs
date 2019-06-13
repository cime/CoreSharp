using System.Security.Principal;

namespace CoreSharp.DataAccess
{
    public interface IUser : IEntity<long>, IPrincipal
    {
        string UserName { get; set; }
    }
}
