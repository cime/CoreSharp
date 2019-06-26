using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<IComplexityConfigurationFactory>(() => new ComplexityConfigurationFactory(), Lifestyle.Singleton);
        }
    }
}
