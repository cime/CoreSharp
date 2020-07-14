using CoreSharp.GraphQL.Configuration;
using GraphQL.Resolvers;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IComplexityConfigurationFactory>(() => new ComplexityConfigurationFactory());
            container.RegisterSingleton<IGraphQLConfiguration>(() => new GraphQLConfiguration());

            container.RegisterSingleton(typeof(AutoInputGraphType<>), typeof(AutoInputGraphType<>).Assembly);
            container.RegisterSingleton(typeof(AutoObjectGraphType<>), typeof(AutoObjectGraphType<>).Assembly);

            container.Register<IFieldResolver>(() => NameFieldResolver.Instance, Lifestyle.Transient);
        }
    }
}
