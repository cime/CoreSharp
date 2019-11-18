using System.Reflection;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace CoreSharp.GraphQL
{
    public class SyntheticPropertyResolver : IFieldResolver
    {
        private readonly PropertyInfo _propertyInfo;

        public SyntheticPropertyResolver(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public object Resolve(ResolveFieldContext context)
        {
            return null;
        }
    }
}
