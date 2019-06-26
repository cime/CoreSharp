using System.Collections.Generic;

namespace CoreSharp.GraphQL
{
    public interface IUserContextBuilder
    {
        IDictionary<string, object> BuildContext();
    }
}
