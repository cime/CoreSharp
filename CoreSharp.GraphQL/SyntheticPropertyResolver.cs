using System.Reflection;
using GraphQL;
using GraphQL.Resolvers;

namespace CoreSharp.GraphQL
{
    public class SyntheticPropertyResolver : IFieldResolver
    {
        private readonly PropertyInfo _propertyInfo;

        public SyntheticPropertyResolver(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public object Resolve(IResolveFieldContext context)
        {
            return null;
        }
    }
}
