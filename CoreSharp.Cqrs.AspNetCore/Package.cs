using SimpleInjector;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<ICqrsOptions>(() => new CqrsOptions(container), Lifestyle.Singleton);
        }
    }
}
