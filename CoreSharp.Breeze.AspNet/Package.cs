using SimpleInjector;

namespace CoreSharp.Breeze.AspNet
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<JsonFormatter, JsonFormatter>(Lifestyle.Singleton);
        }
    }
}
