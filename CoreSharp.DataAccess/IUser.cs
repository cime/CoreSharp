using System.Security.Principal;

namespace CoreSharp.DataAccess
{
    public interface IUser : IEntity<long>
    {
        string UserName { get; set; }
        string Email { get; set; }
        bool Active { get; set; }
    }
}
