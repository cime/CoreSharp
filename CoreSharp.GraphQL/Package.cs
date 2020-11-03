using CoreSharp.GraphQL.Configuration;
using GraphQL.Resolvers;
using GraphQL.Types;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IComplexityConfigurationFactory>(() => new ComplexityConfigurationFactory());
            container.RegisterSingleton<IGraphQLConfiguration>(() => new GraphQLConfiguration());

            container.Register(typeof(AutoInputGraphType<>), typeof(AutoInputGraphType<>));
            container.Register(typeof(AutoObjectGraphType<>), typeof(AutoObjectGraphType<>));
            container.Register(typeof(EnumerationGraphType<>), typeof(EnumerationGraphType<>));


            container.Register<IFieldResolver>(() => NameFieldResolver.Instance, Lifestyle.Transient);
        }
    }
}
