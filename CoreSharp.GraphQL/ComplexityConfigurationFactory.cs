using GraphQL.Validation.Complexity;

namespace CoreSharp.GraphQL
{
    public class ComplexityConfigurationFactory : IComplexityConfigurationFactory
    {
        private readonly ComplexityConfiguration _complexityConfiguration = new ComplexityConfiguration()
        {
            MaxDepth = 15
        };

        public virtual ComplexityConfiguration GetComplexityConfiguration()
        {
            return _complexityConfiguration;
        }
    }
}
