using CoreSharp.GraphQL.Configuration;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<IComplexityConfigurationFactory>(() => new ComplexityConfigurationFactory(), Lifestyle.Singleton);
            container.Register<IGraphQLConfiguration>(() => new GraphQLConfiguration(), Lifestyle.Singleton);

            container.Register(typeof(AutoInputGraphType<>), typeof(AutoInputGraphType<>).Assembly, Lifestyle.Singleton);
            container.Register(typeof(AutoObjectGraphType<>), typeof(AutoObjectGraphType<>).Assembly, Lifestyle.Singleton);
        }
    }
}
