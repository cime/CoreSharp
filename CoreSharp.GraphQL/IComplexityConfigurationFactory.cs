using GraphQL.Validation.Complexity;

namespace CoreSharp.GraphQL
{
    public interface IComplexityConfigurationFactory
    {
        ComplexityConfiguration GetComplexityConfiguration();
    }
}
