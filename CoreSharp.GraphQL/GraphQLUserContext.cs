using System.Collections.Generic;
using CoreSharp.DataAccess;

namespace CoreSharp.GraphQL
{
    public class GraphQLUserContext : IGraphQLUserContext
    {
        public IUser? User { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public IEnumerable<string> Claims { get; set; } = new List<string>();
    }
}
