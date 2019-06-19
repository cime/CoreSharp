using GraphQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreSharp.GraphQL
{
    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string NamedQuery { get; set; }
        public string Query { get; set; }
        [JsonConverter(typeof(InputConverter))]
        public Inputs Variables { get; set; }
    }
}
