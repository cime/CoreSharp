using System.Collections.Generic;
using CoreSharp.DataAccess;

namespace CoreSharp.GraphQL
{
    public interface IGraphQLUserContext
    {
        IUser User { get; set; }
        bool IsAuthenticated { get; set; }
        IEnumerable<string> Claims { get; set; }
    }
}
