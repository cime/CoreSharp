using CoreSharp.Cqrs.AspNetCore.Options;
using SimpleInjector;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<ICqrsOptions>(() => new SimpleInjectorCqrsOptions(container));
            container.RegisterSingleton<CqrsFormatterRegistry>();
            container.RegisterSingleton<CqrsMiddleware>();
        }
    }
}
